# Upgrade Plan

## Attribute Part
- 需要允许用户在不加注ColumnAttribute的情况下使用该类库，以便原有项目切换到该ORM中
	- 仅对Model做TableAttribute标注，加入一个属性用以判断取ColumnAttribute还是属性名称作为数据库字段名称
    - 或者Model不做TableAttribute，一切默认用户已经自行对应完毕
    - 无论怎样都需要通过Property的Type来判断SQL生成时值的格式
    - 有可能需要有个默认字段（例如ID）来作为主键

- 现在已经实现的增删改方法 
  - insert和delete时，不需要主键标注
  - update时，需要有标注的主键，否则程序会抛出异常
## Database Part
- 只需要在程序启动时调用DB.Open方法，传入相应数据库的DbProviderFactory对象和链接字符串即可
- 现在可以支持多个不同数据库在一个程序中链接了，仅需要在Model的TableAttribute中标注数据库名称即可针对某个数据库执行操作
- 