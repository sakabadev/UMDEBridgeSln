using System;
using System.Text.RegularExpressions;
using Demo.Scripts.Master.Item2;
using MessagePack;
using UnityEngine;

namespace Development {
    public class MessagePackTest : MonoBehaviour
    {
        void Start() {
            // CheckTypeName();
            SerializeTest();
            //SerializeTest2();
            // RegexTest();
        }

        void CheckTypeName() {
            int i = 0;
            string s = "string";
            bool b = true;
            float f = 0.1f;
            DateTime dt = DateTime.Now;
            TimeSpan ts = TimeSpan.FromDays(1);
        
            Debug.Log(i.GetType().Name);
            Debug.Log(s.GetType().Name);
            Debug.Log(b.GetType().Name);
            Debug.Log(f.GetType().Name);
            Debug.Log(dt.GetType().Name);
            Debug.Log(ts.GetType().Name);
        }

        void SerializeTest() {
            Item2 item2 = new UnitItem2("100", "最初のアイテム", "", "", 1, 1, SomeType.B);
            Debug.Log($"first json");
            Debug.Log(MessagePackSerializer.SerializeToJson(item2));

            var item = MessagePackSerializer.Serialize(item2);
            string json = MessagePackSerializer.ConvertToJson(item);
            Debug.Log($"second json");
            Debug.Log(json);
        
            var bytes = MessagePackSerializer.ConvertFromJson(json);
            Debug.Log("ConvertFromJson Done.");
            var result = MessagePackSerializer.Deserialize<Item2>(bytes);
            Debug.Log("Done.");
        
            Debug.Log($"{result.Id} {result.Name}");
        
            string json2 = "[0,[\"100\",\"最初のアイテム\",null,null,0,0]]";
            bytes = MessagePackSerializer.ConvertFromJson(json2);
            result = MessagePackSerializer.Deserialize<Item2>(bytes);
            Debug.Log($"{result.Id} {result.Name}");
        
            // string json3 = "[0,[\"0\",\"最初のアイテム\",null,null,\"1\",\"1\"]]";
            string json3 = "[0,[\"0\",\"最初のアイテム\",null,null,1,1]]";
            bytes = MessagePackSerializer.ConvertFromJson(json3);
            result = MessagePackSerializer.Deserialize<Item2>(bytes);
            Debug.Log($"{result.Id} {result.Name}");
        }

        void SerializeTest2() {
            string testJson = "{\"Id\":\"testId\", \"Name\":\"testName\"}";
            string testJson2 = "{\"Id\":\"testId\", \"Name\":\"testName\", \"Text\":\"testText\", \"SE\":1, \"BGM\":1}";
            var obj = new TestMessagePackObject("testId", "testName");
            var bytes = MessagePackSerializer.Serialize(obj);
            string json = MessagePackSerializer.ConvertToJson(bytes);
            Debug.Log(json);
            var obj2 = MessagePackSerializer.Deserialize<TestMessagePackObject>(bytes);
            Debug.Log(obj2.ToString());
            
            var bytes2 = MessagePackSerializer.ConvertFromJson(testJson);
            var obj3 = MessagePackSerializer.Deserialize<TestMessagePackObject>(bytes2);
            Debug.Log(obj3.ToString());
            
            var bytes3 = MessagePackSerializer.ConvertFromJson(testJson2);
            var obj4 = MessagePackSerializer.Deserialize<TestMessagePackObject>(bytes3);
            Debug.Log(obj4.ToString());
        }

        void RegexTest() {
            var reg = new Regex("^idx$|^id$", RegexOptions.IgnoreCase);
            Debug.Log(reg.IsMatch("idx"));
            Debug.Log(reg.IsMatch("id"));
            Debug.Log(reg.IsMatch("Idx"));
            Debug.Log(reg.IsMatch("Id"));
            Debug.Log(reg.IsMatch("IDX"));
            Debug.Log(reg.IsMatch("ID"));
            
            Debug.Log(reg.IsMatch("idxa"));
            Debug.Log(reg.IsMatch("sidx"));
            Debug.Log(reg.IsMatch("ids"));
            Debug.Log(reg.IsMatch("sid"));
        }
    }
    
    // セッター無しでデシリアライズ出来るかどうかテスト（結論は「出来る」）
    [MessagePackObject(true)]
    public sealed class TestMessagePackObject {
        private int se;
        private int bgm;
        
        [SerializationConstructor]
        public TestMessagePackObject() { }

        public TestMessagePackObject(string id, string name, int se = 50, int bgm = 50) {
            Id = id;
            Name = name;
            this.se = se;
            this.bgm = bgm;
        }
        
        public string Id { get; }
        public string Name { get; }
        
        public float SE {
            get => se * 0.01f;
            set => se = (int)(value * 100);
        }
        
        public float BGM {
            get => bgm * 0.01f;
            set => bgm = (int)(value * 100);
        }
        
        public override string ToString() {
            return $"Id: {Id}, Name: {Name}, SE: {SE}, se: {se}, BGM: {BGM}, bgm: {bgm}";
        }
    }
}
