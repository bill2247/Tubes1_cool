using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BOTruutni : Bot
{
    static void Main(string[] args)
    {
        new BOTruutni().Start();
    }

    BOTruutni() : base(BotInfo.FromFile("BOTruutni.json")) { }

    private Random random = new Random();
    private double arenaDiagonal;
    private bool movingForward = true;

    public override void Run()
    {
        // Warna bot
        BodyColor = Color.Cyan;
        GunColor = Color.Black;
        RadarColor = Color.White;

        Console.WriteLine("BOTruutni is running!");

        arenaDiagonal = Math.Sqrt(ArenaWidth * ArenaWidth + ArenaHeight * ArenaHeight);

        while (IsRunning)
        {
            SetTurnRadarLeft(90); // Scan setiap 90 derajat
            MoveAggressive();
            Go(); // Jalankan semua perintah yang telah di-set
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double firePower = ChooseFirePower(distance);

        if (distance < 0.1 * arenaDiagonal)
        {
            // Ramming jika sangat dekat
            Console.WriteLine("Ramming enemy!");
            double angleToEnemy = BearingTo(e.X, e.Y);
            SetTurnLeft(angleToEnemy - Direction);
            SetForward(distance - 10);
        }
        else
        {
            // Sederhanakan perhitungan tembakan
            double angleToEnemy = BearingTo(e.X, e.Y);
            SetTurnGunLeft(angleToEnemy - GunDirection);
            SetFire(firePower);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Hit wall! Reversing...");
        ReverseDirection(); // Panggil fungsi untuk mundur
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Hit another bot! Reversing...");
        ReverseDirection();
    }

    private void MoveAggressive()
    {
        SetForward(40000); // Bergerak jauh agar tidak terjebak di dinding
        SetTurnLeft(random.Next(0, 360)); // Pilih arah acak
    }

    private void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            SetForward(40000);
            movingForward = true;
        }
    }

    private double ChooseFirePower(double distance)
    {
        if (distance < 0.2 * arenaDiagonal)
            return 3;
        else if (distance < 0.3 * arenaDiagonal)
            return 2.5;
        else if (distance < 0.4 * arenaDiagonal)
            return 2;
        else if (distance < 0.5 * arenaDiagonal)
            return 1.5;
        else if (distance < 0.6 * arenaDiagonal)
            return 1;
        else
            return 0.5;
    }
}
