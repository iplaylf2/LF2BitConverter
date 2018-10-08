# LF2BitConverter
BitConverter的扩展版。用于自定义对象与Byte[]互转，支持大小端，支持使用特性控制转换细节,支持继承特性扩展功能。    
     
使用方式如下：
```
var bytes= BitConverterEX.LittleEndian.GetBytes(obj);

var i=0;
var newObj=BitConverterEX.LittleEndian.ToObject<T>(bytes,ref i);
```
更多demo在demo文件夹
