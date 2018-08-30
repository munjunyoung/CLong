using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FishScript : FoodBase
{
    protected override void Awake()
    {
        fillHealthAmount = 50;
        eatingTime = 2;
    }

}

