using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BOTentahlah : Bot
{
    static void Main(string[] args)
    {
        new BOTentahlah().Start();
    }

    BOTentahlah() : base(BotInfo.FromFile("BOTentahlah.json")) { }

    private double enemyDistance = double.MaxValue;

    public override void Run()
    {
        BodyColor = Color.Red;
        GunColor = Color.Black;
        RadarColor = Color.White;

        Console.WriteLine("BOTentahlah is running!");

        MaxSpeed=8; // Kecepatan maksimum

        while (IsRunning)
        {
            TurnRadarRight(360);
            MoveLinear();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        enemyDistance = distance;

        // Jika musuh sangat dekat, atur posisi dulu baru ramming
        if (distance < 50)
        {
            Console.WriteLine("Adjusting for ramming!");
            double angleToEnemy = BearingTo(e.X, e.Y);
            double turnAngle = angleToEnemy - Direction;
            TurnRight(turnAngle);

            Console.WriteLine("Ramming enemy!");
            MaxSpeed=4; // Perlambat untuk belok dengan presisi
            Forward(distance - 10);
            MaxSpeed=8; // Percepat kembali setelah menabrak
        }
        else if (distance >= 50 && distance <= 600)
        {
            // Tembak dengan aturan firepower berdasarkan jarak
            double firePower = ChooseFirePower(distance);

            double angleToEnemy = BearingTo(e.X, e.Y);
            double gunTurn = angleToEnemy - GunDirection;
            TurnGunRight(gunTurn);

            Fire(firePower);
            Console.WriteLine("Shooting with power: " + firePower);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Hit wall! Adjusting path...");
        MaxSpeed=4; // Perlambat agar tidak kena damage besar
        Back(50);
        TurnRight(90);
        MaxSpeed=8; // Percepat lagi setelah menghindari dinding
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Hit another bot! Adjusting...");
        Back(30);
        TurnRight(45);
    }

    private void MoveLinear()
    {
        // Jika mendekati dinding, perlambat dan ubah arah
        if (X < 51 || X > ArenaWidth - 51 || Y < 51 || Y > ArenaHeight - 51)
        {
            // MaxSpeed=4; // Perlambat sebelum terlalu dekat
            Back(100);
            TurnRight(90);
            MaxSpeed=8; // Percepat lagi setelah menjauh dari dinding
            Console.WriteLine("Avoid wall");
        }
        else
        {
            MaxSpeed=8;
            Forward(300);
            // Rescan();
        }
    }

    private double ChooseFirePower(double distance)
    {
        if (distance >= 50 && distance < 100)
            return 3;
        else if (distance >= 100 && distance < 200)
            return 2.5;
        else if (distance >= 200 && distance < 300)
            return 2;
        else if (distance >= 300 && distance < 400)
            return 1.5;
        else if (distance >= 400 && distance < 500)
            return 1;
        else if (distance >= 500 && distance < 600)
            return 0.5;
        else
            return 0;
    }
}
