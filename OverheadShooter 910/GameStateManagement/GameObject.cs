using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace OverheadShooter
{
    class GameObject // used to draw my objects and place.
    {
        public Model model = null;
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 velocity = Vector3.Zero;
        public bool alive = false;
        public float scale = 1.0f;
        public float special = 0f; //Used differently for different objects; might be a timer, a size, a counter, etc
        public int projType = 0; //projectile type, for things like TestCollision
    }


}
