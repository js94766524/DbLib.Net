### 关于DbMaster中使用Attribute批量处理数据的方法之性能比较

#### 说明

DbMaster中实现了两组批量处理数据的方法，分别是：

```

public void InsertModelList<D>(IEnumerable<D> modelList)
public void InsertModelList_UserParameter<D>(IEnumerable<D> modelList)

public void UpdateModelList<D>(IEnumerable<D> modelList)
public void UpdateModelList_UserParameter<D>(IEnumerable<D> modelList)

```

其中，不使用DbParameter的方法是在一次性生成全部SQL语句之后调用数据库连接进行事务处理；
而使用DbParameter的方法是首先生成带有标志的SQL语句，在开启事务之后，遍历model集合生成DbParameter数组，每生成一个DbParameter数组就执行一次操作。

可想而知，不使用DbParameter的方法生成大量含有重复内容的SQL语句，是对内存资源的一种浪费，而使用DbParameter的方法虽然解决了string类型的SQL语句内存浪费的问题，但是却有损效率。

#### 测试数据

以DbMaster.InsertModelList(_UseParameter)为例，我分别对两个方法进行了三次不同数据量的测试:

- 数据量：100,000 items

方法 | 内存(M) | 耗时(ms) | 时间效率(ms/item)
---|---|---|---
不使用DbParameter | 51 | 11,969 | 0.11969
使用DbParameter | 4 | 40,527  | 0.40527

- 数据量：200,000 items

方法 | 内存(M) | 耗时(ms) |时间效率(ms/item)
---|---|---|---
不使用DbParameter | 96 | 24,019|0.120095
使用DbParameter | 6 | 50,399|0.251995

- 数据量：500,000 items

方法 | 内存(M) | 耗时(ms) |时间效率(ms/item)
---|---|---|---
不使用DbParameter | 223 | 62,626|0.125252
使用DbParameter | 4 | 148,883|0.297766

#### 结论

两种方法各有优缺点，请根据具体情况选用合适的方法。