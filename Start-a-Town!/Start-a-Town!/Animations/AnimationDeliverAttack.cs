using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    class AnimationDeliverAttack
    {
        static public Animation Create(GameObject entity, Action onWeaponContact, Action onFinish)
        {
            var ani = new Animation(entity, "Attacking");

            var handani = new AnimationClip(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                     new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
                    new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp)
                );

            ani.Add(BoneDef.RightHand, handani);

            var weapon = 
                new AnimationClip(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)
                );
            ani.Add(BoneDef.Mainhand, weapon);

            ani.Add(BoneDef.Torso,
                new AnimationClip(WarpMode.Clamp,
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f)
                    ));
            return ani;
        }
    }
}
