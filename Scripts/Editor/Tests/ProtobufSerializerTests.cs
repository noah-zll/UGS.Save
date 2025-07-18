//------------------------------------------------------------
// UGS Save System
// Copyright © 2023 UGS Team. All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using NUnit.Framework;
using ProtoBuf;
using UnityEngine;

namespace UGS.Save.Tests
{
    /// <summary>
    /// Protobuf序列化器测试
    /// </summary>
    public class ProtobufSerializerTests
    {
        private ProtobufSaveSerializer _serializer;
        
        [SetUp]
        public void Setup()
        {
            _serializer = new ProtobufSaveSerializer();
        }
        
        [Test]
        public void SerializeAndDeserialize_SimpleData_Success()
        {
            // 准备测试数据
            var testData = new TestData
            {
                stringValue = "Hello Protobuf",
                intValue = 42,
                floatValue = 3.14f,
                boolValue = true
            };
            
            // 序列化
            string serialized = _serializer.Serialize(testData);
            
            // 确保序列化结果不为空
            Assert.IsNotEmpty(serialized);
            
            // 反序列化
            var deserialized = _serializer.Deserialize<TestData>(serialized);
            
            // 验证数据
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testData.stringValue, deserialized.stringValue);
            Assert.AreEqual(testData.intValue, deserialized.intValue);
            Assert.AreEqual(testData.floatValue, deserialized.floatValue);
            Assert.AreEqual(testData.boolValue, deserialized.boolValue);
        }
        
        [Test]
        public void SerializeAndDeserialize_ComplexData_Success()
        {
            // 准备测试数据
            var testData = new ComplexTestData
            {
                name = "Complex Test",
                values = new List<int> { 1, 2, 3, 4, 5 },
                nestedData = new TestData
                {
                    stringValue = "Nested Data",
                    intValue = 100,
                    floatValue = 9.9f,
                    boolValue = false
                }
            };
            
            // 序列化
            string serialized = _serializer.Serialize(testData);
            
            // 确保序列化结果不为空
            Assert.IsNotEmpty(serialized);
            
            // 反序列化
            var deserialized = _serializer.Deserialize<ComplexTestData>(serialized);
            
            // 验证数据
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testData.name, deserialized.name);
            
            // 验证列表
            Assert.IsNotNull(deserialized.values);
            Assert.AreEqual(testData.values.Count, deserialized.values.Count);
            for (int i = 0; i < testData.values.Count; i++)
            {
                Assert.AreEqual(testData.values[i], deserialized.values[i]);
            }
            
            // 验证嵌套对象
            Assert.IsNotNull(deserialized.nestedData);
            Assert.AreEqual(testData.nestedData.stringValue, deserialized.nestedData.stringValue);
            Assert.AreEqual(testData.nestedData.intValue, deserialized.nestedData.intValue);
            Assert.AreEqual(testData.nestedData.floatValue, deserialized.nestedData.floatValue);
            Assert.AreEqual(testData.nestedData.boolValue, deserialized.nestedData.boolValue);
        }
        
        [Test]
        public void SerializeAndDeserialize_UnityVector3_Success()
        {
            // 准备测试数据
            var testData = new UnityTypesTestData
            {
                position = new SerializableVector3(new Vector3(1.1f, 2.2f, 3.3f))
            };
            
            // 序列化
            string serialized = _serializer.Serialize(testData);
            
            // 确保序列化结果不为空
            Assert.IsNotEmpty(serialized);
            
            // 反序列化
            var deserialized = _serializer.Deserialize<UnityTypesTestData>(serialized);
            
            // 验证数据
            Assert.IsNotNull(deserialized);
            
            // 验证Vector3
            Vector3 originalVector = testData.position.ToVector3();
            Vector3 deserializedVector = deserialized.position.ToVector3();
            
            Assert.AreEqual(originalVector.x, deserializedVector.x, 0.001f);
            Assert.AreEqual(originalVector.y, deserializedVector.y, 0.001f);
            Assert.AreEqual(originalVector.z, deserializedVector.z, 0.001f);
        }
    }
    
    /// <summary>
    /// 简单测试数据类
    /// </summary>
    [ProtoContract]
    public class TestData
    {
        [ProtoMember(1)]
        public string stringValue;
        
        [ProtoMember(2)]
        public int intValue;
        
        [ProtoMember(3)]
        public float floatValue;
        
        [ProtoMember(4)]
        public bool boolValue;
    }
    
    /// <summary>
    /// 复杂测试数据类
    /// </summary>
    [ProtoContract]
    public class ComplexTestData
    {
        [ProtoMember(1)]
        public string name;
        
        [ProtoMember(2)]
        public List<int> values;
        
        [ProtoMember(3)]
        public TestData nestedData;
    }
    
    /// <summary>
    /// Unity类型测试数据
    /// </summary>
    [ProtoContract]
    public class UnityTypesTestData
    {
        [ProtoMember(1)]
        public SerializableVector3 position;
    }
    
    /// <summary>
    /// 可序列化的Vector3
    /// </summary>
    [ProtoContract]
    public struct SerializableVector3
    {
        [ProtoMember(1)]
        public float x;
        
        [ProtoMember(2)]
        public float y;
        
        [ProtoMember(3)]
        public float z;
        
        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}