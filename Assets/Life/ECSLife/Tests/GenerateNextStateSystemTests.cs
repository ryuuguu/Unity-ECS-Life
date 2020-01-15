using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;
using Unity.Mathematics;


namespace Tests {
    [TestFixture]
    //[Category("ECS Test")]
    public class GenerateNextStateSystemTests:ECSTestsFixture {
        [Test]
        public void WhenDeadAndNEquals3() {
            var _cells = new Entity[3,3];
        
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    var instance = m_Manager.CreateEntity();
                    m_Manager.AddComponentData(instance, new Live { value = 0});
                    m_Manager.AddComponentData(instance, new PosXY { pos = new int2(x,y)});
                    _cells[x, y] = instance;
                }
            }

            var i = 1;
            var j = 1;
            var center = _cells[i,j ];
            m_Manager.SetComponentData(_cells[0,0 ], new Live { value = 1});
            m_Manager.SetComponentData(_cells[0,1 ], new Live { value = 1});
            m_Manager.SetComponentData(_cells[0,2 ], new Live { value = 1});
                
            m_Manager.AddComponentData(center, new NextState() {value = 0});
            m_Manager.AddComponentData(center, new Neighbors() {
                nw = _cells[i - 1, j - 1], n = _cells[i - 1, j], ne =  _cells[i - 1, j+1],
                w = _cells[i , j-1], e = _cells[i, j + 1],
                sw = _cells[i + 1, j - 1], s = _cells[i + 1, j], se =  _cells[i + 1, j + 1]
            });
            
            World.CreateSystem<GenerateNextStateSystem>().Update();

            Assert.AreEqual(1, m_Manager.GetComponentData<NextState>(center).value);

        }
    }
}
