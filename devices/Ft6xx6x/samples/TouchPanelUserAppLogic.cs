using System;
using System.Diagnostics;

namespace NFAppCore2App2
{
    public class TouchPanelUserAppLogic
    {
        public void TouchPanelUserAppLogicSkeleton(object sender, TouchEventArgs e)
        {
            const string strLB = "LEFT BUTTON PRESSED";
            const string strMB = "MIDDLE BUTTON PRESSED";
            const string strRB = "RIGHT BUTTON PRESSED";
            const string strXY1 = "TOUCH PANEL TOUCHED at X= ";
            const string strXY2 = ",Y= ";
            const string strXY1XY2 = "DOUBLE TOUCH X1Y1 X2Y2";

            Debug.WriteLine("Touch Panel Event Received Category= " + e.TouchEventCategory.ToString() + " Subcategory= " + e.TouchEventSubCategory.ToString());
            
            nanoFramework.Console.Clear();

            if (e.TouchEventSubCategory == (int)TouchEventSubcategory.LeftButton)
            {
                Debug.WriteLine(strLB);
                nanoFramework.Console.WriteLine(strLB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.MiddleButton)
            {
                Debug.WriteLine(strMB);
                nanoFramework.Console.WriteLine(strMB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.RightButton)
            {
                Debug.WriteLine(strRB);
                nanoFramework.Console.WriteLine(strRB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.SingleTouch)
            {
                Debug.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());
                nanoFramework.Console.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.DoubleTouch)
            {
                Debug.WriteLine(strXY1XY2);

            }
            else
            {
                Debug.WriteLine("ERROR: UKNOWN TouchEventSubcategory");
            }

        }

    }
}
