using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    class AnimationDeliverAttack
    {
        //public Action OnWeaponContact = () => { };
        //public Action OnFinish = () => { };

        static public Animation Create(GameObject entity, Action onWeaponContact, Action onFinish)
        {
            var ani = new Animation(entity, "Attacking");
            //ani.Layer = 4;

            var handani = new AnimationClip(WarpMode.Once,
                    //new Keyframe(0, Vector2.Zero, -(float)Math.PI),
                    //new Keyframe(100, Vector2.Zero, 0, Interpolation.Exp)

                    new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                     //new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
                     //new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                     new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
                    new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp)
                );
            //handani.AddAction(10, onWeaponContact);
            //handani.AddAction(20, onFinish);

            ani.Add(BoneDef.RightHand, handani);

            var weapon = //Mainhand,
                new AnimationClip(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)
                // new Keyframe(10, Vector2.Zero, 0, Interpolation.Exp)
                );
            ani.Add(BoneDef.Mainhand, weapon);

            ani.Add(BoneDef.Torso,
                new AnimationClip(WarpMode.Clamp,
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f)

                    //     new Keyframe(10, Vector2.Zero, 0)
                    ));
            return ani;
        }

        //private void WeaponContact()
        //{
        //    this.OnWeaponContact();
        //}
        //private void Finish()
        //{
        //    this.OnFinish();
        //}
    }

    //class AnimationDeliverAttack : AnimationCollection
    //{
    //    public Action OnWeaponContact = () => { };
    //    public Action OnFinish = () => { };

    //    //public AnimationDeliverAttack(Action onWeaponContact):this()
    //    //{
    //    //    this.OnWeaponContact = onWeaponContact;
    //    //}
    //    public AnimationDeliverAttack()
    //        : base("Attacking")
    //    {
    //        this.Layer = 4;

    //        var handani = new Animation(WarpMode.Once,
    //            //new Keyframe(0, Vector2.Zero, -(float)Math.PI),
    //            //new Keyframe(100, Vector2.Zero, 0, Interpolation.Exp)

    //                new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
    //            //new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
    //            //new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
    //                 new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
    //                new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp)
    //            );
    //        handani.AddAction(10, () => WeaponContact());
    //        handani.AddAction(20, () => Finish());

    //        this.Add(BoneDef.RightHand, handani);

    //        var weapon = //Mainhand,
    //            new Animation(WarpMode.Once,
    //                new Keyframe(0, Vector2.Zero, 0),
    //                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
    //                new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)
    //            // new Keyframe(10, Vector2.Zero, 0, Interpolation.Exp)
    //            );
    //        this.Add(BoneDef.Mainhand, weapon);

    //        this.Add(BoneDef.Torso,
    //            new Animation(WarpMode.Clamp,
    //                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
    //                new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
    //                new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f)

    //           //     new Keyframe(10, Vector2.Zero, 0)
    //                ));
    //    }

    //    private void WeaponContact()
    //    {
    //        this.OnWeaponContact();
    //    }
    //    private void Finish()
    //    {
    //        this.OnFinish();
    //    }
    //}
}
