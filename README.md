# LF2BitConverter
System.BitConverter的扩展版。用于自定义对象与Byte[]互转。   

## 用法

接口简单，支持大小端转换。   

``` C#
//Convert字段顺序为类型定义时的顺序
//转换的类型也是定义时的类型
class Foo
{
    public Int32 A;
    public Int32 B;
}

void Demo()
{
    Foo foo = new Foo { A = 1, B = 2 };

    Byte[] littleEndian_bytes = BitConverterEX.LittleEndian.GetBytes(foo);  //new Byte[]{1,0,0,0,2,0,0,0}
    Byte[] bigEndian_bytes = BitConverterEX.BigEndian.GetBytes(foo);  //new Byte[]{0,0,0,1,0,0,0,2}

    Int32 startIndex = 0;
    Foo new_foo_1 = BitConverterEX.LittleEndian.ToObject<Foo>(littleEndian_bytes, ref startIndex);  //{A=1,B=2}

    startIndex = 0;
    Foo new_foo_2 = BitConverterEX.BigEndian.ToObject<Foo>(bigEndian_bytes, ref startIndex);    //{A=1,B=2}
}
```

默认提供转换方法的类型为System.BitConverter支持的类型,以及Byte。    

``` C#
class Foo
{
    public Double A;
}
```

支持数组转换。    

``` C#
class Foo
{
    /// <summary>
    /// 按元素个数计数，固定长度为10的数组
    /// </summary>
    [ConvertArray(CountBy.Item, Length = 10)]
    public Int32 FooArray;

    /// <summary>
    /// 存放BarArray getbytes后的字节数
    /// </summary>
    public Int32 BarCount;

    /// <summary>
    /// 按getbytes后字节长度计数，不定长度的数组
    /// </summary>
    [ConvertArray(CountBy.Byte, LengthFrom = nameof(BarCount))]
    public Char[] BarArray;
}
```

支持嵌套类型。     

``` C#
class Foo
{
    public Int32 A;
    public Double B;
}

class Bar
{
    /// <summary>
    /// 甚至支持嵌套类型的数组。
    /// </summary>
    [ConvertArray(CountBy.Item, Length = 10)]
    public Foo[] FooArray;    
}
```

## 说明
使用表达式树动态构造转换函数，性能与手写转换代码相当。   

更多demo在demo目录。    