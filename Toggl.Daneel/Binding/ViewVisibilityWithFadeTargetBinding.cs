﻿using Toggl.Daneel.Extensions;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;
using Toggl.Foundation.MvvmCross.Helper;
using System.Threading.Tasks;

namespace Toggl.Daneel.Binding
{
    public sealed class ViewVisibilityWithFadeTargetBinding : MvxTargetBinding<UIView, bool>
    {
        public const string BindingName = "VisibilityWithFade";

        public ViewVisibilityWithFadeTargetBinding(UIView target) : base(target)
        {
            target.Opaque = false;
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            var alreadyVisible = Target.Alpha > 0.5f;
            if (value == alreadyVisible) return;

            var alpha = value ? 1 : 0;

            Task.Delay(1).ContinueWith(_ =>
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    AnimationExtensions.Animate(
                        Animation.Timings.EnterTiming,
                        Animation.Curves.EaseIn,
                        () => Target.Alpha = alpha
                    );
                });
            });
        }
    }
}
