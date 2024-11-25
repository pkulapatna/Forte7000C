using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000C
{
    public static class ClsUiMsg
    {
        //Scale is inside of the test cell Action 1. Initialize Read Upcount
        public static string ReadUpCount = "Select Product Type, DO NOT PLACE BALE ON THE SCALE!, Press Initialize, " +
                                         "Wait for Read to Appear";

        //Scale is inside of the test cell Action 2. Read Scale and Read DownCount
        public static string ReadScaleDownCount = "Place Product on Scale, Wait for Scale to stabilize then Press Read.";

        //Scale is outside of the test cell Action 1.Initialize Read Scale and Read UpCount
        public static string ReadScaleUpCount = "Select Product Type, Place Bale on Scale!, Press Initialize, " +
                                        "Wait for Read to appear";

        //Scale is outside of the test cell Action 2. Read DownCount
        public static string ReadDownCount = "Place Product in Test Cell, then Press Read.";
    }
}
