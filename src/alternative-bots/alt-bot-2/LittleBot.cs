using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// java -jar robocode-tankroyale-gui-0.30.0.jar
public class LittleBot : Bot{   
    static void Main(string[] args)
    {
        new LittleBot().Start();
    }

    LittleBot() : base(BotInfo.FromFile("LittleBot.json")) { }

    public override void Run()
    {
        double arenaWidth = ArenaWidth;
        double arenaHeight = ArenaHeight;
        bool IsInCircle = false;
        // Warna Bot
        BodyColor = Color.HotPink;
        RadarColor = Color.MediumPurple;
        GunColor = Color.White;
        TracksColor = Color.DeepPink;
        BulletColor = Color.Red;

        while(!IsInCircle){
            MoveToAreaOfCenter(arenaWidth, arenaHeight);
            if(Math.Pow((X-arenaWidth/2),2) + Math.Pow((Y-arenaHeight/2),2) < Math.Pow(80,2)){
                Console.WriteLine("Yeay sudah di tengah hehe!!!!!!!!!");
                IsInCircle = true;
            }else{
                Console.WriteLine("Belum di tengah, coba lagi");
                IsInCircle = false;
            }
        }

        while (IsRunning){
            while(!IsInCircle){
                MoveToAreaOfCenter(arenaWidth, arenaHeight);
                if(Math.Pow((X-arenaWidth/2),2) + Math.Pow((Y-arenaHeight/2),2) < Math.Pow(80,2)){
                    Console.WriteLine("Yeay sudah di tengah hehe!!!!!!!!!");
                    IsInCircle = true;
                }else{
                    Console.WriteLine("Belum di tengah, coba lagi");
                    IsInCircle = false;
                }
            }
            Console.WriteLine("Saatnya berputar!!!");
            // Tell the game that when we take move, we'll also want to turn right... a lot
            SetTurnLeft(5_000);
            // Limit our speed to 9
            MaxSpeed = 10;
            // Start moving (and turning)
            Forward(10_000);
        }
    }

    public void MoveToAreaOfCenter(double arenaWidth, double arenaHeight){
        Console.WriteLine("Menuju tengah!!!");
        if(X < arenaWidth/2){
            if(Y<arenaHeight/2){
                TurnRight(Direction - 45);
            }else{
                TurnRight(Direction + 45);
            }
        }else{
            if(Y<arenaHeight/2){
                TurnRight(Direction + 225);
            }else{
                TurnRight(Direction + 135);
            }
        }
        // Hitung jarak ke tengah arena
        double distance = DistanceTo(arenaWidth / 2, arenaHeight / 2);
        MaxSpeed = 10;
        Forward(distance); // Bergerak ke tengah
    }

    public override void OnHitWall(HitWallEvent e){
        double arenaWidth = ArenaWidth;
        double arenaHeight = ArenaHeight;
        Console.WriteLine("Nabrak dinding euyyyy");
        // Bounce off!
        TurnRight(75);
        // Bergerak menjauhi tembok setelah berputar
        Forward(400);
        MoveToAreaOfCenter(arenaWidth, arenaHeight);
    }

    // ReverseDirection: Switch from ahead to back & vice versa

    // We scanned another bot -> fire hard!
    public override void OnScannedBot(ScannedBotEvent e)
    {
        var distanceWithOtherBot = DistanceTo(e.X, e.Y);
        if(distanceWithOtherBot >400){
            if(e.Speed == 0){
                Console.WriteLine("Tembak Jauh dua kali");
                Fire(1);
                Fire(1);
            }else{
                Console.WriteLine("Tembak Jauh satu kali");
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
            if(e.Speed >4){
                Console.WriteLine("Tembak Sedang");
                Fire(2);
            }else{
                Console.WriteLine("Tembak Dekat");
                Fire(3);
            }
        }else{
            Console.WriteLine("Tembak Sangat Dekat");
            Fire(4);
            Fire(1);
        }
    }

    // We hit another bot -> if it's our fault, we'll stop turning and moving,
    // so we need to turn again to keep spinning.
    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -10 && bearing < 10){
            Console.WriteLine("Di depan mata ditabrak");
            Forward(25);
            Fire(3);
        }
        if (e.IsRammed){
            Console.WriteLine("Ditabrak duhhh sakittt");
            TurnLeft(10);
        }
    }
    /* Read the documentation for more events and methods */
}
