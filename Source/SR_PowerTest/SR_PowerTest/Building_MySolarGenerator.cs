using RimWorld;
using UnityEngine;
using Verse;
namespace SR
{
    public class Building_MySolarGenerator : Building
    {
        private const float fullSunPower = 1700f;//日间最大发电量
        private const float nightPower = 0f;//夜间发电量
        //遮挡因数
        private float roofedFactor {
            get {
                int cells = 0;//建筑面积中占用的坐标总数
                int roofedCells = 0;//建筑被屋顶挡住的坐标数量
                foreach (var c in GenAdj.OccupiedRect(this.Position,this.Rotation,this.def.size))
                {
                    ++cells;
                    //被屋顶挡住了 影响太阳能效率
                    if (Map.roofGrid.Roofed(c))
                    {
                        ++roofedCells;
                    }
                }
                return (float)(cells-roofedCells) / (float)cells;
            }
        }
        private float realPower => (fullSunPower-nightPower)*Map.skyManager.CurSkyGlow * roofedFactor;//实际发电量=(最大发电-最小发电)*天气因数*遮挡因数
        private CompPowerTrader compPowerTrader;//电力交换组件
        /// <summary>
        /// 生成时回调
        /// </summary>
        /// <param name="map"></param>
        /// <param name="respawningAfterLoad"></param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>() ?? new CompPowerTrader();//寻找电力交换组件
            compPowerTrader.PowerOutput = 0;
        }
        /// <summary>
        /// 每帧做什么
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            compPowerTrader.PowerOutput = realPower;//每一帧提供实际的电力 如果你想做的是电池的话，PowerOutput可以传负值进去
        }
        /// <summary>
        /// 销毁时的回调
        /// </summary>
        /// <param name="mode"></param>
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }
        /// <summary>
        /// 绘制方法
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            GenDraw.FillableBarRequest fbr= default(GenDraw.FillableBarRequest); //初始化一个填充条
            fbr.center = DrawPos + Vector3.up * 0.1f;//绘制中心
            fbr.size = new Vector2(2.3f, 0.14f);//尺寸
            fbr.fillPercent = realPower / fullSunPower;//填充比例
            fbr.filledMat= SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f), false);//填充部分的颜色
            fbr.unfilledMat= SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);//未填充部分的颜色
            fbr.margin = 0.15f;//间距
            Rot4 rot4 = Rotation;//旋转
            rot4.Rotate(RotationDirection.Clockwise);//调整一下素材旋转度
            fbr.rotation = rot4;
            GenDraw.DrawFillableBar(fbr);//出来吧电量条!
        }
    }
}