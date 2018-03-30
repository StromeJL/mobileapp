﻿using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.CalendarRecyclerView")]
    public sealed class CalendarRecyclerView : MvxRecyclerView
    {
        public CalendarRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public CalendarRecyclerView(Context context)
            : this(context, null)
        {
        }

        public CalendarRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public CalendarRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new CalendarRecyclerAdapter())
        {
            SetLayoutManager(new GridLayoutManager(context, 7, LinearLayoutManager.Vertical, false));
        }
    }
}