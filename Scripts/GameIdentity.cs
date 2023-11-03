using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoTerrain.Scripts {
    public class GameIdentity{
        public GameIdentity(string identityName, Vector2 position, bool centeredOrigin = true) {
            GameIdentity identity = new GameIdentity(identityName, centeredOrigin: centeredOrigin);
            identity.Transform.position = position;
        }

        public GameIdentity(string identityName = "", string texture = "", int renderOrder = 0, bool centeredOrigin = true) {
            Name = identityName == string.Empty ? "NewGameIdentity" : identityName;
            
            GameIdentityManager.Instance.CreatedIdentities++;
            IdentityId = GameIdentityManager.Instance.CreatedIdentities;

            Texture2D loadedTexture = null;
            try {
                loadedTexture = GameController.Instance.Content.Load<Texture2D>(texture);
            }
            catch { }

            Children = new List<GameIdentity>();
            Transform = new Transform();

            if (loadedTexture != null) { 
                Visual = new GameVisual(loadedTexture, Color.White);
                if (centeredOrigin) Transform.originOffset = new Vector2(loadedTexture.Width / 2f, loadedTexture.Height / 2f);
            }

            RenderOrder = renderOrder;
            Active = true;
        }

        public string Name { get; set; }
        public int IdentityId { get; set; }

        public bool Active { get; set; }
        public int RenderOrder { get; set; }

        public Transform Transform { get; set; }
        public GameVisual Visual { get; set; }
        public List<GameIdentity> Children { get; set; }
    }

    public class GameVisual {
        public GameVisual(Texture2D targetTexture, Color textureColor) {
            this.targetTexture = targetTexture;
            this.textureColor = textureColor;
        }

        public Color textureColor = Color.White;
        public Texture2D targetTexture;
    }

    public class Transform {
        public float rotation = 0;
        public Vector2 position = Vector2.Zero;
        public Vector2 scale = Vector2.One;
        public Vector2 originOffset = Vector2.Zero;

        public void SetScale(Vector2 newScale) {
            scale = newScale;
        }
    }
}
