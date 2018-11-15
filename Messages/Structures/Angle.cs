﻿using System;

namespace TeraPacketRetranslator.Messages
{
    public struct Angle
    {
        private readonly int _raw;

        public Angle(int raw)
            : this()
        {
            _raw = raw;
        }

        public double Radians => _raw*(2*Math.PI/0x10000);
        public int Gradus => _raw*360/0x10000;
        public int Raw => _raw;

        public static Angle operator + (Angle x, Angle y) { return new Angle(x.Raw + y.Raw); }
        public static Angle operator - (Angle x, Angle y) { return new Angle(x.Raw - y.Raw); }
        public static Angle operator - (Angle x) { return new Angle(-x.Raw ); }
        public static bool operator > (Angle x, Angle y) { return x.Raw > y.Raw; }
        public static bool operator < (Angle x, Angle y) { return x.Raw < y.Raw; }
        public static bool operator ==(Angle x, Angle y) { return x.Raw == y.Raw; }
        public override bool Equals(object obj) { return _raw == (obj as Angle?)?.Raw; }
        public override int GetHashCode() { return _raw; }
        public static bool operator !=(Angle x, Angle y) { return x.Raw != y.Raw; }
        public static bool operator >=(Angle x, Angle y) { return x.Raw >= y.Raw; }
        public static bool operator <=(Angle x, Angle y) { return x.Raw <= y.Raw; }
        public static Angle operator +(Angle x, int y) { return new Angle(x.Raw + y); }
        public static Angle operator -(Angle x, int y) { return new Angle(x.Raw - y); }

        public override string ToString()
        {
            return $"{Gradus}";
        }

        public static Angle Normalize(Angle angle)
        {
            return new Angle((angle.Raw + 0x8000) % 0x10000 - 0x8000);
        }

        public static bool CheckSide(Angle posAngle, Angle attAngle)
        {
            posAngle = Normalize(posAngle);
            attAngle = Normalize(attAngle);
            if (posAngle.Raw < 0) { posAngle = -posAngle; attAngle = -attAngle; }
            if (posAngle.Raw > 0x4000) return false;
            if ((-0x6000 >= attAngle.Raw) && (attAngle > posAngle - 0xA000)) return true;
            if (posAngle.Raw < 0x2000 && attAngle >= Normalize(posAngle - 0xA000)) return true;
            return false;
        }

        public static HitDirection GetHitDirection(Vector3f myPos, Angle myAngle, Vector3f bossPos, Angle bossAngle)
        {
            var posAngle = bossPos.GetHeading(myPos) - bossAngle;
            var attAngle = myAngle - bossAngle;
            if (CheckSide(posAngle - 0x8000, attAngle - 0x8000))  return HitDirection.Back;
            if (CheckSide(posAngle - 0x4000, attAngle - 0x4000))  return HitDirection.Right | HitDirection.Side;
            if (CheckSide(posAngle + 0x4000, attAngle + 0x4000))  return HitDirection.Left  | HitDirection.Side;
            return HitDirection.Front;
        }

        public HitDirection GetHitDirection(Angle target)
        {
            var diff = (target.Gradus - Gradus + 720)%360;
            var side = diff > 180 ? HitDirection.Left : HitDirection.Right;
            if (diff > 180) diff = 360 - diff;
            if (diff <= 55) return HitDirection.Back;
            if (diff >= 125) return HitDirection.Front;
            return HitDirection.Side|side;
        }
    }
}