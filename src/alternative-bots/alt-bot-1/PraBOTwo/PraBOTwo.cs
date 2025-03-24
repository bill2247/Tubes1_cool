using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System.Collections.Generic;

public class PraBOTwo : Bot
{
    private int moveDirection = 1;
    private Dictionary<int, List<EnemyData>> enemyHistory = new Dictionary<int, List<EnemyData>>();
    private int currentTargetId = -1;
    private const double OrbitalDistance = 200; //distance to orbit around enemy
    private const double AggressionThreshold = 50;
    private Random random = new Random();

    private class EnemyData
    {
        public double Time { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Energy { get; set; }
        public double Velocity { get; set; }
        public double Heading { get; set; }
    }

    public PraBOTwo() : base(BotInfo.FromFile("PraBOTwo.json")) { }

    static void Main(string[] args)
    {
        new PraBOTwo().Start();
    }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0xFF, 0x14, 0x93);
        TurretColor = Color.FromArgb(0x00, 0xFF, 0x00);
        RadarColor = Color.FromArgb(0xFF, 0xA5, 0x00);
        BulletColor = Color.FromArgb(0x8A, 0x2B, 0xE2);

        /* Strategy: 
         * 1. Target one enemy at a time 
         * 2. Fire at the enemy based on their energy level, the lower the energy, the higher the bullet power
         * 3. If the enemy is high energy, spin/orbit and shoot from a distance. If too close, move away with a random turn
         * 4. If the enemy is low energy, move closer for the kill
         * 5. Preserve energy by reducing bullet power when energy is low and enemy has high energy
         * 6. Avoid walls by moving towards the center of the arena
         */ 
        while (IsRunning)
        {
            SetTurnRight(30);
            SetForward(100);
            SetTurnRadarLeft(360);
            ScanTargets();
            AdjustMovement();
            AvoidWalls();
            Go();
        }
    }

    
    private void ScanTargets() // Scans for targets
    {
        // If no target is found, scan the area
        if (currentTargetId == -1 || !enemyHistory.ContainsKey(currentTargetId))
        {
            SetTurnRadarLeft(360);
        }
        else
        {
            var targetData = enemyHistory[currentTargetId][enemyHistory[currentTargetId].Count - 1];
            double radarTurn = NormalizeRelativeAngle(RadarBearingTo(targetData.X, targetData.Y));
            SetTurnRadarLeft(radarTurn);

            if (TurnNumber - targetData.Time > 5)
            {
                currentTargetId = -1; // assume target is lost
            }
        }
    }

    // Adjusts movement based on the current target
    // If the target is high energy, orbit from distance and shoot
    // If the target is low energy, move closer
    private void AdjustMovement()
    {
        if (currentTargetId != -1 && enemyHistory.ContainsKey(currentTargetId))
        {
            var targetData = enemyHistory[currentTargetId][enemyHistory[currentTargetId].Count - 1];

            if (targetData.Energy < AggressionThreshold)
            {
                double angleToTarget = NormalizeRelativeAngle(DirectionTo(targetData.X, targetData.Y) - Direction);
                double orbitalOffset = 45;
                SetTurnLeft(angleToTarget + orbitalOffset);
                SetForward(100);
            }
            else
            {
                double distance = DistanceTo(targetData.X, targetData.Y);
                if (distance < OrbitalDistance)
                {
                    double randomTurn = random.Next(-30, 30);
                    SetTurnLeft(randomTurn);
                    SetForward(50);
                }
            }
        }
        else
        {
            double randomTurn = random.Next(30, 30);
            SetTurnRight(randomTurn);
            SetForward(100 * moveDirection);
            moveDirection *= -1;
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        int enemyId = evt.ScannedBotId;
        if (!enemyHistory.ContainsKey(enemyId))
        {
            enemyHistory[enemyId] = new List<EnemyData>();
        }

        enemyHistory[enemyId].Add(new EnemyData
        {
            Time = TurnNumber,
            X = evt.X,
            Y = evt.Y,
            Energy = evt.Energy,
            Velocity = evt.Speed,
            Heading = evt.Direction
        });

        if (enemyHistory[enemyId].Count > 10)
        {
            enemyHistory[enemyId].RemoveAt(0);
        }

        if (currentTargetId == -1 || ShouldSwitchTarget(evt))
        {
            currentTargetId = enemyId;
        }

        double bulletPower = CalculateBulletPower(evt.Energy);
        if (bulletPower > 0)
        {
            double predictedX = evt.X + evt.Speed * Math.Cos(NormalizeAbsoluteAngle(evt.Direction));
            double predictedY = evt.Y + evt.Speed * Math.Sin(NormalizeAbsoluteAngle(evt.Direction));
            double fireAngle = NormalizeRelativeAngle(GunBearingTo(predictedX, predictedY));
            SetTurnGunLeft(fireAngle);

            if (Math.Abs(GunTurnRemaining) < 5)
            {
                SetFire(bulletPower);
                SetTurnRight(20);
                SetForward(100);
            }
        }
    }

    // Calculate bullet power based on the target's energy level
    // Higher energy targets get hit with lower power bullets and vice versa
    // Preserve energy by reducing bullet power when energy is low and enemy has high energy
    private double CalculateBulletPower(double targetEnergy)
    {
        if (Energy < 10 && EnemyCount < 3 && targetEnergy > 20) {
            return 1.0;
        }
        if (targetEnergy > 70) return 1.5;
        if (targetEnergy > 50) return 2.0;
        if (targetEnergy > 30) return 2.5;
        return 3.0;
    }

    //Strategy: Switch target if the new found enemy has lower energy
    private bool ShouldSwitchTarget(ScannedBotEvent newEnemy)
    {
        if (currentTargetId == -1) return true;

        var currentTargetData = enemyHistory[currentTargetId][enemyHistory[currentTargetId].Count - 1];
        return newEnemy.Energy < currentTargetData.Energy;
    }

    private void AvoidWalls()
    {
        double margin = 100;
        if (X < margin || X > ArenaWidth - margin || Y < margin || Y > ArenaHeight - margin)
        {
            double angleToCenter = NormalizeRelativeAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2) - Direction);
            SetTurnLeft(angleToCenter);
            if (TurnRemaining < 10)
            {
                SetForward(100);
            }
        }
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        double randomTurn = random.Next(-45, 45);
        SetTurnLeft(randomTurn);
        SetForward(50 * moveDirection);
        moveDirection *= -1;
    }

     public override void OnHitBot(HitBotEvent evt)
    {
        SetTurnLeft(30);
        SetBack(100); // Move backward to avoid further collisions
        Go(); 
    }

    public override void OnRoundStarted(RoundStartedEvent roundStartedEvent)
    {
        moveDirection = 1;
        currentTargetId = -1;
    }
}