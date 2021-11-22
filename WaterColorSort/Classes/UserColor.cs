﻿using System;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal struct UserColor : IComparable
    {
        internal readonly Color color;
        internal readonly string name;

        internal UserColor(Color color, string name = "")
        {
            this.color = color;
            this.name = name;
        }

        public static implicit operator Color(UserColor color) => color.color;

        public static explicit operator UserColor(Color color) => new(color);

        public override bool Equals(object obj) => obj is UserColor color && color.color == this.color;

        public static bool operator ==(UserColor color1, UserColor color2) => color1.Equals(color2);

        public static bool operator !=(UserColor color1, UserColor color2) => !color1.Equals(color2);

        public override int GetHashCode() => HashCode.Combine(color, name);

        int IComparable.CompareTo(object obj)
        {
            return name.CompareTo(((UserColor)obj).name);
        }
    }
}
