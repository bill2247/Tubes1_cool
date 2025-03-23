using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;

public class BOTanchan : Bot
{
    private int moveDirection = 1; // 1 = forward, -1 = backward
    private ScannedBotEvent lastScannedEnemy;
    private Dictionary<int, List<EnemyData>> enemyHistory = new Dictionary<int, List<EnemyData>>();
    private int currentTargetId = -1; // ID of the current target
    private const double WallMargin = 125; // Distance to start avoiding walls

    private void AvoidWalls() // Reducing the chance of hitting the walls to preserve energy and survive longer
    {
        // Calculate the distance each side of arena
        double distanceToLeftWall = X;
        double distanceToRightWall = ArenaWidth - X;
        double distanceToTopWall = Y;
        double distanceToBottomWall = ArenaHeight - Y;

        // Determine the closest wall
        double minDistance = Math.Min(Math.Min(distanceToLeftWall, distanceToRightWall), Math.Min(distanceToTopWall, distanceToBottomWall));

        // Adjust direction to avoid the wall
        if (minDistance < WallMargin)
        {
            double centerAngle = DirectionTo(ArenaWidth / 2, ArenaHeight / 2);

            if (distanceToLeftWall < WallMargin)
            {
                // Add turn angle to avoid getting stuck in the left wall
                double turnAngle = 20;
                SetTurnLeft(centerAngle + turnAngle);
            }
            else
            {
                // Go towards the center of the arena
                SetTurnLeft(centerAngle);
            }
            SetForward(100);
        }
    }

    // Enemy data structure
    private class EnemyData
    {
        public double Time { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Velocity { get; set; }
        public double Heading { get; set; }
    }

    static void Main(string[] args)
    {
        new BOTanchan().Start();
    }

    BOTanchan() : base(BotInfo.FromFile("BOTanchan.json")) { }

    public override void Run()
    {
        // Set bot colors
        BodyColor = Color.FromArgb(0x1E, 0x90, 0xFF);   // Dodger Blue
        TurretColor = Color.FromArgb(0x00, 0xBF, 0xFF); // Deep Sky Blue
        RadarColor = Color.FromArgb(0x00, 0x8B, 0x8B);  // Dark Cyan
        BulletColor = Color.FromArgb(0xDC, 0x14, 0x3C); // Crimson
        ScanColor = Color.FromArgb(0x7C, 0xFC, 0x00);   // Lawn Green
        TracksColor = Color.FromArgb(0x2F, 0x4F, 0x4F); // Dark Slate Gray
        GunColor = Color.FromArgb(0x8A, 0x2B, 0xE2);    // Blue Violet

        while (IsRunning)
        {
            /*
            Strategy: Gain as many points as possible by going aggressive forward and dodging bullets with
            unpredictable movements. If energy is low, be less aggressive but still attacking especially when
            there is only one enemy left.
            */
            if (Energy < 30)
            {
                // Retreat mode
                moveDirection = -1;
                SetTurnLeft(45 * Math.Sin(TurnNumber * 0.3)); // Random Pattern to make it harder for enemies to hit
                if (EnemyCount == 1 || Energy < 10)
                {
                    moveDirection = 1; // If only one enemy left, switch to attack mode or low energy to reduce hitting the wall
                }
                SetForward(80 * moveDirection); // Move back and forward to create distance
            }
            else
            {
                // Attack mode: move toward enemy with randomness
                moveDirection = 1;
                SetTurnLeft(30 * Math.Sin(TurnNumber * 0.2) + new Random().Next(-15, 15)); // Add randomness to movement to avoid being predictable
                SetForward(100 * moveDirection); // Move forward to engage enemy
            }

            // Wall avoidance (reducing it)
            AvoidWalls();

            // Radar logic
            /*
            Strategy: Aggressively targeting a single enemy while continuously scanning for new targets if the current target is lost.

            1. If a target is locked:
               - Continuously track the target by turning the radar toward its last known position.
               - If no new scan data is received for 3 turns, perform a small sweep (45 degrees) to re-acquire the target.
               - If no new scan data is received for 5 turns, assume the target is lost and reset the current target.
            2. If no target is locked:
               - Perform continuous small sweeps (45 degrees) to search for new targets.
            */
            if (currentTargetId != -1 && enemyHistory.ContainsKey(currentTargetId))
            {
                if (EnemyCount < 4) {
                    AdjustRadarForBodyTurn = true;
                }
                var targetData = enemyHistory[currentTargetId];
                if (targetData.Count > 0)
                {
                    var lastData = targetData[targetData.Count - 1];
                    double radarTurn = NormalizeRelativeAngle(RadarBearingTo(lastData.X, lastData.Y));
                    SetTurnRadarLeft(radarTurn);

                    if (TurnNumber - lastData.Time > 3)
                    {
                        SetTurnRadarRight(45); // Search for the current target
                    }
                    // If no new scan after a while, assume target is lost
                    if (TurnNumber - lastData.Time > 5)
                    {
                        currentTargetId = -1; // Reset target
                        AdjustRadarForBodyTurn = false;
                    }
                }
            }
            else
            {
                // Sweep radar continuously when no target is locked
                SetTurnRadarLeft(45);
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Store enemy data for predictive targeting
        int enemyId = evt.ScannedBotId;
        if (!enemyHistory.ContainsKey(enemyId))
        {
            enemyHistory[enemyId] = new List<EnemyData>();
        }

        // Calculate enemy velocity and heading
        // Strategy: Use the difference between the current and previous positions to estimate velocity and heading.
        double velocity = evt.Speed;
        double heading = evt.Direction;
        if (enemyHistory[enemyId].Count >= 1)
        {
            EnemyData prevData = enemyHistory[enemyId][enemyHistory[enemyId].Count - 1];
            double dt = TurnNumber - prevData.Time;
            double dx = evt.X - prevData.X;
            double dy = evt.Y - prevData.Y;
            heading = Math.Atan2(dy, dx) * (180 / Math.PI); // Convert radians to degrees
        }

        // Add new enemy data
        enemyHistory[enemyId].Add(new EnemyData
        {
            Time = TurnNumber,
            X = evt.X,
            Y = evt.Y,
            Velocity = velocity,
            Heading = heading
        });

        if (enemyHistory[enemyId].Count > 10)
        {
            enemyHistory[enemyId].RemoveAt(0);
        }

        lastScannedEnemy = evt;

        // Switch target if no target is currently locked, or new enemy is closer than the current target
        // Strategy: being greedy by choosing closer enemies to be more accurate and likely to hit.
        if (currentTargetId == -1 || ShouldSwitchTarget(evt))
        {
            currentTargetId = enemyId;
        }

      
        // Predict enemy position.
        double distance = DistanceTo(evt.X, evt.Y);
        double bulletPower = 0;

        // Strategy: Prioritize firing at closer enemies with higher bullet power
        // Adjust bullet power based on distance and energy levels to maximize damage while conserving energy.
        if (distance <= 100 && Energy > 20.0)
        {
            bulletPower = 3.0; // Max power for close range and high energy
        }
        else if (distance <= 150 && Energy > 15.0)
        {
            bulletPower = 2.5; // Strong shot for medium range and moderate energy
        }
        else if (distance <= 200 && Energy > 10.0)
        {
            bulletPower = 2.0; // Medium shot for longer range and low energy
        }
        else if (distance <= 300 && Energy > 10.0)
        {
            bulletPower = 1.5; // Weak shot for long range and very low energy
        }
        else if (distance <= 400 && Energy > 10.0)
        {
            bulletPower = 1.0; // Minimal shot for extreme range and critical energy
        }
        else if (distance <= 200 && Energy <= 10.0)
        {
            bulletPower = 1.0; // Conservative shot for low energy and medium range
        }
        else if (distance <= 400 && Energy <= 10.0)
        {
            bulletPower = 0.5; // Minimal shot for low energy and long range
        }

        if (bulletPower != 0)
        {
            double bulletSpeed = CalcBulletSpeed(bulletPower);
            double timeToTarget = distance / bulletSpeed;

            // Predict future position using linear targeting
            double futureX = evt.X + velocity * timeToTarget * Math.Cos(NormalizeAbsoluteAngle(heading));
            double futureY = evt.Y + velocity * timeToTarget * Math.Sin(NormalizeAbsoluteAngle(heading));

            double targetAngle = GunBearingTo(futureX, futureY);
            SetTurnGunLeft(targetAngle);

            if (distance > 400)
            {
                // Strategy: If the enemy is far away, adjust the bot's movement to close the distance while aiming.
                SetTurnLeft(targetAngle);
                if (TurnRemaining < 10)
                {
                    SetForward(50);
                }
            }

            // Strategy: Ensure the gun is properly aligned before firing to conserve energy and avoid wasting bullets.
            if (GunTurnRemaining < 5)
            {
                SetFire(bulletPower);
                SetTurnLeft(10 + new Random().Next(-10, 10)); // Add randomness to movement to avoid predictability
                SetForward(100);
            }
        }

        Go(); 
    }

    private bool ShouldSwitchTarget(ScannedBotEvent newEnemy)
    {
        if (currentTargetId == -1) return true; // No current target

        // Get current target data
        if (!enemyHistory.ContainsKey(currentTargetId) || enemyHistory[currentTargetId].Count == 0)
            return true; 

        var currentTargetData = enemyHistory[currentTargetId][enemyHistory[currentTargetId].Count - 1];

        // Switch target if the new enemy is closer
        double currentDistance = DistanceTo(currentTargetData.X, currentTargetData.Y);
        double newDistance = DistanceTo(newEnemy.X, newEnemy.Y);

        return newDistance < currentDistance;
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        SetTurnLeft(45);
        SetForward(50);
        Go(); 
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        // Immediate correction with center-seeking
        double centerAngle = DirectionTo(ArenaWidth / 2, ArenaHeight / 2);
        SetTurnLeft(centerAngle);
        SetForward(100);

        Go();
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        SetTurnLeft(45);
        SetBack(100); // Move backward to avoid further collisions
        Go(); 
    }

    public override void OnRoundStarted(RoundStartedEvent roundStartedEvent)
    {
    moveDirection = 1; // Default to forward
    currentTargetId = -1;
    }
}