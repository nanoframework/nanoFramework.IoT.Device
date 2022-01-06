// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.Runtime.Events;

namespace NFAppCore2App2
{
    public class TouchPanelDiscriminator
    {
        const double pxPerMm = 6.90415;
        //r in milimeters is the radious of sensitivity to detect any of the given 3 buttons
        const double rMm = 3.5;
        const int x0L = 83;
        const int x0M = 182;
        const int x0R = 271;
        const int y0 = 263;
        //r^2 in pixels^2
        const int r2 = (int)((rMm * pxPerMm) * (rMm * pxPerMm));

        public static void DiscrimitateButtons(int x, int y)
        {
             TouchEventArgs touchEventArgs = new TouchEventArgs();

            //WARNING: Next assigment should be EventCategory.Touch but Touch Category not yet defined on main/develop
            touchEventArgs.TouchEventCategory = EventCategory.Unknown;
            touchEventArgs.X1 = x;
            touchEventArgs.Y1 = y;

            int tmpL = 0;
            int tmpM = 0;
            int tmpR = 0;
            int tmpY = 0;

            tmpY = (y - y0) * (y - y0);
            tmpL = (x - x0L) * (x - x0L);
            tmpL += tmpY;
            tmpM = (x - x0M) * (x - x0M);
            tmpM += tmpY;
            tmpR = (x - x0R) * (x - x0R);
            tmpR += tmpY;

            if (tmpL <= r2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.LeftButton;
            }
            else if (tmpM <= r2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.MiddleButton;
            }
            else if (tmpR <= r2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.RightButton;
            }
            else
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.SingleTouch;
            }

            TouchPanelProcessor.OnTouchedEvent(touchEventArgs.TouchEventSubCategory, touchEventArgs.X1, touchEventArgs.Y1);

        }
    }
}

