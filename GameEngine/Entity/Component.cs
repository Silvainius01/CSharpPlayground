using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Component
    {
        public Entity entity { get; internal set; }

        public virtual void Awake() { }

        public virtual void Start() { }
        public virtual void Update() { }

        public virtual void OnDestroy() { }

        internal void Destroy()
        {
            OnDestroy();
            entity = null;
        }
    }
}
