﻿using System;
using Foundation;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(TextViewWithPlaceholder))]
    public class TextViewWithPlaceholder : UITextView, IUITextViewDelegate
    {
        private readonly int defaultPlaceholderSize = 14;
        private readonly UIColor defaultPlaceholderColor = Color.Common.PlaceholderText.ToNativeColor();

        private bool isFocused;
        private UIStringAttributes defaultTextAttributes;
        private UIStringAttributes placeholderAttributes;

        public event EventHandler TextChanged;

        private UIColor placeholderColor;
        public UIColor PlaceholderColor
        {
            get => placeholderColor;
            set
            {
                placeholderColor = value;
                updatePlaceholderStyle();
            }
        }

        private nfloat placeholderSize;
        public nfloat PlaceholderSize
        {
            get => placeholderSize;
            set
            {
                placeholderSize = value;
                updatePlaceholderStyle();
            }
        }

        private string text;
        public override string Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;
                
                text = value;
                updateAttributedText(value);
                TextChanged.Raise(this);
            }
        }

        public string PlaceholderText { get; set; }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Delegate = this;

            PlaceholderSize = defaultPlaceholderSize;
            PlaceholderColor = defaultPlaceholderColor;

            defaultTextAttributes = new UIStringAttributes
            {
                ParagraphStyle = new NSMutableParagraphStyle { Alignment = TextAlignment },
                Font = UIFont.SystemFontOfSize(Font.PointSize)
            };

            updateAttributedText(Text);
        }

        private void updatePlaceholderStyle()
        {
            placeholderAttributes = new UIStringAttributes
            {
                ParagraphStyle = new NSMutableParagraphStyle { Alignment = TextAlignment },
                ForegroundColor = PlaceholderColor,
                Font = UIFont.SystemFontOfSize(PlaceholderSize)
            };

            updateAttributedText(Text);
        }

        public TextViewWithPlaceholder(IntPtr handle) : base(handle)
        {
        }

        private void updateAttributedText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                AttributedText = isFocused
                    ? new NSAttributedString("")
                    : new NSAttributedString(PlaceholderText ?? "", placeholderAttributes);

                return;
            }

            AttributedText = new NSAttributedString(text, defaultTextAttributes);
        }

        [Export("textView:shouldChangeTextInRange:replacementText:")]
        public new bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (text == Environment.NewLine)
            {
                ResignFirstResponder();
                return false;
            }

            return true;
        }

        [Export("textViewDidChange:")]
        public new void Changed(UITextView textView)
        {
            Text = base.Text.Replace(Environment.NewLine, " ");
        }

        [Export("textViewDidBeginEditing:")]
        public void EditingStarted(UITextView textView)
        {
            isFocused = true;
            updateAttributedText(Text);
        }

        [Export("textViewDidEndEditing:")]
        public void EditingEnded(UITextView view)
        {
            isFocused = false;
            updateAttributedText(Text);
        }
    }
}