using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace MyTestApp{
    static class Config
    {
        // 設定値関係のクラス
        public static readonly int MaxPopPointNum = 5;
        public static readonly double StartPopPointAngle = 0D;
        public static readonly double EndPopPointAngle = 180D;
        public static readonly double PopPointDistance = 10D;
    }
    class Program
    {
        static void Main(string[] args)
        {
            // 5箇所の出現ポイントを作成する
            var popPointList = new List<PopPoint>();
            for(int i = 0; i < Config.MaxPopPointNum; i++)
            {
                var angle = Config.StartPopPointAngle 
                    + (Config.EndPopPointAngle - Config.StartPopPointAngle)
                       /(Config.MaxPopPointNum - 1) * i;
                var location = new Point3D((int)angle, Config.PopPointDistance);
                var p = new PopPoint(i, location);
                Console.WriteLine("Point {0}: x = {1}, y = {2}, z = {3}", 
                    i, p.Location.x, p.Location.y, p.Location.z);
                popPointList.Add(p);
            }
            // それぞれのポイントでアバターを抽選
            for(int i = 0; i < Avatar.MaxAvatarType * Avatar.MaxAvatarAttributeType; i++)
            {
                Console.Write("{0} 回目：", i + 1);
                foreach(PopPoint p in popPointList)
                {
                    Avatar avatar = p.PopNewAvatar();
                    Console.Write("  {0}  ", avatar.AvatarName);
                }
                Console.WriteLine();
            }

        }
    }

    // 3D座標はUnityの場合はVector3とか使うけどここは省略
    class Point3D
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point3D(int angle, double distance)
        {
            this.SetPosition(angle, distance);
        }

        public void SetPosition(int angle, double distance)
        {
            // 自分の座標を　x＝0、y＝0、z＝0 とし、角度と距離で座標を初期化する。
            // z軸は0固定で
            this.z = 0D;
            var rad = (angle / 180D) * Math.PI;
            //　ラジアン変換の際に誤差が出るのが気持ち悪いので90度単位で固定値にする
            if((angle % 180) == 0){
                this.x = (angle % 360) == 0 ? distance : -1 * distance;
                this.y = 0D;
            }else if((angle % 90) == 0){
                this.x = 0D;
                this.y = ((angle / 90) % 4) == 3 ? -1 * distance: distance;
            }else{
                // x=rcosθ
                this.x = distance * Math.Cos(rad);
                // y=rsinθ
                this.y = distance * Math.Sin(rad);
            }
        }
    }

    class PopPoint
    {
        public Point3D Location {get;}
        public int PointNo {get;}
        private int poppedAvatarNum;
        private bool[] usedAvatar = new bool[Avatar.MaxAvatarType * Avatar.MaxAvatarAttributeType];
        private Random rnd = new Random();

        public PopPoint(int pointNo, Point3D location){
            this.PointNo = pointNo;
            this.Location = location;
            this.poppedAvatarNum = 0;
            this.initRandom();
        }

        private void initRandom()
        {
            // 一応同じ乱数を発生しない様に初期化する, 1217,1223は素数
            int seed = (Environment.TickCount / 1223) * 1217 + this.PointNo;
            this.rnd = new Random(seed);
        }


        public Avatar PopNewAvatar()
        {
            int avatarPatternNum = Avatar.MaxAvatarType * Avatar.MaxAvatarAttributeType;
            if((poppedAvatarNum % avatarPatternNum) == 0)
            {
                //全種類出し尽くしたのでアバター出現管理の配列を初期化する
                for(int i = 0; i < avatarPatternNum; i++)
                {
                    this.usedAvatar[i] = false;
                }
                this.poppedAvatarNum = 0;
            }
            int foundAvatarIndex = -1;
            while(true)
            {
                int avatarIndex = this.rnd.Next(avatarPatternNum);
                if(!this.usedAvatar[avatarIndex])
                {
                    foundAvatarIndex = avatarIndex;
                    break;
                }
            }
            // あり得ないケース
            Debug.Assert(foundAvatarIndex != -1, "アバターの抽選に失敗");
            this.usedAvatar[foundAvatarIndex] = true;
            this.poppedAvatarNum ++;
            int avatarType = (int)(foundAvatarIndex / Avatar.MaxAvatarAttributeType);
            int avatarAttributeType = (foundAvatarIndex % Avatar.MaxAvatarAttributeType);
            Avatar avatar = new Avatar(avatarType, avatarAttributeType);
            return avatar;
        }
    }

    class Avatar
    {
        public static readonly string [] AvatarTypeString 
            = new string[] 
                {
                    "AVA-001",
                    "AVA-002",
                    "AVA-003",
                    "AVA-004"
                };
        public static readonly string [] AvatarAttributeTypeString 
            = new string[] 
                {
                    "M:1",
                    "M:2",
                    "F:1",
                    "F:2"
                };
        public static readonly int MaxAvatarType = AvatarTypeString.Length;
        public static readonly int MaxAvatarAttributeType = AvatarAttributeTypeString.Length;
        public string AvatarName { get;}
        public int AvaterType { get;}
        public int AvatarAttributeType { get;}

        public Avatar(int avatarType, int avatarAttributeType)
        {
            // avatarType : 0 〜 MaxAvatarType
            // avatarAttributeType : 0 〜 MaxAvatarAttributeType
            avatarType = avatarType < 0 ? 0 : avatarType;
            avatarAttributeType = avatarAttributeType < 0 ? 0 : avatarAttributeType;
            this.AvaterType = avatarType < MaxAvatarType ? avatarType: 0;
            this.AvatarAttributeType = avatarAttributeType < MaxAvatarAttributeType ? avatarAttributeType: 0;
            this.AvatarName = AvatarTypeString[this.AvaterType] 
                + ":" + AvatarAttributeTypeString[this.AvatarAttributeType];
        }

    }
}