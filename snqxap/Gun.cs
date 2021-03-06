﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snqxap
{

    class Equip
    {
        public string name { get; set; }
        public double critup { get; set; }
        public int breakarmor { get; set; }
        public int damage { get; set; }
        public int shotspeed { get; set; }
        public int hit { get; set; }
        public double nightsee { get; set; }
        public int dodge { get; set; }
        public int rank { get; set; }
        public int belt { get; set; }
        public int forwhat {get;set;}
        /// <summary>
        /// 1光瞄/2全息/3acog/4夜视/5穿甲/6震撼/7燃烧/8高速/9芯片/10外骨/11插板/12勋章
        /// </summary>
        public int type { get; set; }

        public string tooltip { get; set; }
    }

    class Gun
    {
        public string image { get; set; }
        public int number { get; set; }
        public int effect0 { get; set; }
        public int effect1 { get; set; }
        public int effect2 { get; set; }
        public int effect3 { get; set; }
        public int effect4 { get; set; }
        public int effect5 { get; set; }
        public int effect6 { get; set; }
        public int effect7 { get; set; }
        public double damageup { get; set; }
        public double shotspeedup { get; set; }
        public double hitup { get; set; }
        public double dodgeup { get; set; }
        public double critup { get; set; }
        public double rateup { get; set; }
        public int to { get; set; } // all 1 ar 2 smg 3 no 0 
        public string name { get; set; } //名字
        public double crit { get; set; }//暴击率
        /// <summary>
        /// 2 ar 3 smg 4 hg 5 rf 6 mg    
        /// </summary>
        public int what { get; set; }   
        public int belt { get; set; }//弹链

        public double ratiohp { get; set; }//hp
        public double ratiopow { get; set; }//伤害
        public double ratiohit { get; set; } //命中
        public double eatratio { get; set; }
        public double ratiorate { get; set; } //射速
        public double ratiododge { get; set; }//闪避

        public int probability {get;set;}
        public double skilleffect1 { get; set; }
        public double skilleffect2 { get; set; }
        public double skilleffect3 { get; set; }
        public double skilleffect4 { get; set; }
        public double growth { get; set; }
        public int growth_type { get;set;}
        public int type { get; set; } 
        public Equip equip { get; set; }

    }

    public class GunGrid
    {
        public double damageup { get; set; }
        public double shotspeedup { get; set; }
        public double hitup { get; set; }
        public double dodgeup { get; set; }
        public double critup { get; set; }
        public double rateup { get; set; }


        public void cleargg()
        {
            this.critup = 1.00;
            this.damageup = 1.00;
            this.dodgeup = 1.00;
            this.hitup = 1.00;
            this.shotspeedup = 1.00;
            this.rateup = 0.00;
        }
    }

}
