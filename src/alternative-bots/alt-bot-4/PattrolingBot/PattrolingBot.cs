using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// java -jar robocode-tankroyale-gui-0.30.0.jar
public class PattrolingBot : Bot{   
    static void Main(string[] args)
    {
        new PattrolingBot().Start();
    }

    PattrolingBot() : base(BotInfo.FromFile("PattrolingBot.json")) { }

    public override void Run()
    {
        double arenaWidth = ArenaWidth;
        double arenaHeight = ArenaHeight;
        // Warna Bot
        BodyColor = Color.Black;
        RadarColor = Color.Black;
        GunColor = Color.White;
        TracksColor = Color.Black;
        BulletColor = Color.Green;

        // Berputar ke arah bawah arena
        if(Direction <= 270){
            TurnLeft(270 - Direction);
        }else{
            TurnRight(Direction - 270);
        }

        // Bergerak ke bawah arena
        Forward(Y - 25);
        TurnGunLeft(90);

        //Bergerak ke pojok kanan bawah
        TurnLeft(90);
        Forward(arenaWidth - X - 25);

        while (IsRunning){
            Console.WriteLine("Jaga ronda euy!!!");
            TurnLeft(90);
            TurnGunRight(90);
            TurnGunLeft(90);
            Forward(arenaHeight -25);
            TurnLeft(90);
            TurnGunRight(90);
            TurnGunLeft(90);
            Forward(arenaWidth -25);
            TurnLeft(90);
            TurnGunRight(90);
            TurnGunLeft(90);
            Forward(arenaHeight -25);
            TurnLeft(90);
            TurnGunRight(90);
            TurnGunLeft(90);
            Forward(arenaWidth -25);
        }
    }

    public override void OnHitWall(HitWallEvent e){
        Console.WriteLine("Kepentok Boyyy");
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var distanceWithOtherBot = DistanceTo(e.X, e.Y);
        if(distanceWithOtherBot >400){
            if(e.Speed == 0){
                Console.WriteLine("Tembak Jauh dua kali");
                SetFire(1);
                SetFire(1);
                SetFire(1);
            }else{
                Console.WriteLine("Tembak Jauh satu kali");
                Fire(1);
                Fire(1);
            }
        }else if(distanceWithOtherBot >200){
            if(e.Speed >6){
                Console.WriteLine("Tembak Jauh");
                Fire(1);
            }else{
                Console.WriteLine("Tembak Sedang");
                Fire(2);
            }
        }else if(distanceWithOtherBot >50){
            if(e.Speed >3){
                Console.WriteLine("Tembak Sedang");
                Fire(2);
            }else{
                Console.WriteLine("Tembak Dekat");
                Fire(3);
            }
        }else{
            Console.WriteLine("Tembak Sangat Dekat");
            Fire(1);
            Fire(3);
            Fire(1);
        }
    }

    public override void OnHitBot(HitBotEvent e){
        if(e.IsRammed){
            Console.WriteLine("Ditabrak duhhh sakittt hehehe");
        }else{
            Console.WriteLine("Ditabrak duhhh sakittt");
            Fire(2);
            Fire(1);
        }
    }

    public override void OnHitByBullet(HitByBulletEvent e){
        Console.WriteLine("Ditembak duhhh sakittt");
    }
}
