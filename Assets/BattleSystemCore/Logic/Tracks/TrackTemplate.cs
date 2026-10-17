namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 语义角色引用：模板里的目标不存具体实体id（配置写在战斗之前，彼时id不存在），
    /// 只能用关系表达，触发时刻由TrackContext解析成具体id。
    /// 技能表与Buff触发表共用同一套轨道模板，同一语义在两侧含义一致。
    /// </summary>
    public enum RoleRef
    {
        /// <summary>触发者：普攻命中事件的攻击者 / 技能施放者</summary>
        Caster = 1,

        /// <summary>作用目标：被打的人 / 技能目标 / 位移承受者</summary>
        Target = 2,

        /// <summary>Buff持有者（触发载体所在Actor；技能场景等于Caster）</summary>
        Owner = 3,
    }

    /// <summary>轨道类型标识。工厂注册表的key，配置层与运行时轨道类的唯一对应</summary>
    public enum TrackType
    {
        DoDamage = 1,
        ApplyBuff = 2,
        Move = 3,

        /// <summary>形状攻击：按判定形状对范围内单位结算伤害（AoE/普攻扇形）</summary>
        AttackShape = 4,

        /// <summary>击飞：对目标施加垂直抛物线（可叠伤害/位移组合成浮空连）</summary>
        Knockup = 5,

        /// <summary>生成子弹：发射一发弹体交由ProjectileManager飞行与命中结算；
        /// 弹体参数可引模板（projectileId）+内联覆盖（bulletSpeed/Radius/Range，0=用模板值），
        /// 或无模板全内联；bulletLabel>0时注册句柄供ControlBullet操控</summary>
        SpawnBullet = 6,

        /// <summary>操控子弹：持续轨道，在durationTicks内每帧按句柄重设指定弹的飞行方向
        /// （追踪target / 固定向量x,y,z），只改向不改速</summary>
        ControlBullet = 7,

        /// <summary>触发特效：纯信号轨道——只广播表现事件（FxTriggered/FxStopped），
        /// 零战斗状态改动、不进状态哈希。durationTicks≤1=瞬时（一次性特效），
        /// ≥2=持续（Exit时广播停止，打断补Exit即“打断即停”）</summary>
        TriggerFx = 8,
    }

    /// <summary>操控子弹的方向来源（模板存int，工厂直转，照AttackShapeType惯例）</summary>
    public enum BulletCtrlMode
    {
        /// <summary>追踪：每帧朝target当前位置飞行（目标死亡/重合则保持原向）</summary>
        FollowTarget = 0,

        /// <summary>固定向量：每帧重设为x,y,z方向（原始向量，内部归一化）</summary>
        FixedDirection = 1,
    }

    /// <summary>AttackShape的判定心来源（模板存int，工厂直转，照BulletCtrlMode惯例）</summary>
    public enum ShapeCenterMode
    {
        /// <summary>跟随origin角色：以origin解析的actor当前位置为判定心（默认，任意上下文通用）</summary>
        FollowOrigin = 0,

        /// <summary>弹着点：以上下文携带的弹当前位置为判定心（命中=接触点、超程=落点）。
        /// 仅弹的payload上下文携带该坐标；无弹着点时工厂回退FollowOrigin</summary>
        ImpactPoint = 1,
    }

    /// <summary>
    /// 轨道模板：可序列化的轨道载荷描述（纯数据，无委托、无实体id、无Vec3——
    /// 位移以x/y/z标量表达，工厂实例化时组装）。技能与Buff触发共用同一套词汇。
    /// 轨道从"运行时容器"降维为"效果载荷"：模板是静态数据，实例化发生在触发时刻。
    /// </summary>
    public class TrackTemplate
    {
        public TrackType type;

        /// <summary>相对触发tick的偏移：0=触发当帧（瞬时轨道同帧结算），正值=延迟Ntick生效</summary>
        public int relTick;

        /// <summary>持续时长（tick），仅持续型轨道（Move）使用，最小1；瞬时轨道忽略</summary>
        public int durationTicks = 1;

        /// <summary>行为发起者（DoDamage的攻击方）</summary>
        public RoleRef origin = RoleRef.Caster;

        /// <summary>行为承受者（挨打方/被位移方/被上buff方）</summary>
        public RoleRef target = RoleRef.Target;

        /// <summary>ApplyBuff：目标Buff的id（经BuffLibrary解析为BuffData）</summary>
        public uint buffId;

        /// <summary>Move：总位移分量（千分单位，可为负）。配置层标量，工厂组装Vec3</summary>
        public int x;

        /// <summary>Move：y分量</summary>
        public int y;

        /// <summary>Move：z分量</summary>
        public int z;

        /// <summary>AttackShape：判定形状类型（AttackShapeType数值直转：1圆/2扇/3矩形）</summary>
        public int shapeType;

        /// <summary>AttackShape：半径（milli，圆/扇共用）</summary>
        public int shapeRadius;

        /// <summary>AttackShape：扇形半角余弦阈值（万分比，10000=仅正前，0=±90°）</summary>
        public int shapeMinDot;

        /// <summary>AttackShape：矩形半宽（milli，x方向）</summary>
        public int shapeHalfX;

        /// <summary>AttackShape：矩形半深（milli，z方向）</summary>
        public int shapeHalfZ;

        /// <summary>AttackShape：判定心模式（ShapeCenterMode数值直转：0=跟随origin角色/1=上下文弹着点）。
        /// 弹着点仅在弹的命中/超程载荷中生效；技能直放等无弹着点上下文回退origin</summary>
        public int shapeCenterMode;

        /// <summary>Knockup：垂直初速（milli/tick，向上为正）</summary>
        public int vy0;

        /// <summary>Knockup：重力（milli/tick²）</summary>
        public int gravity;

        /// <summary>SpawnBullet：弹体定义id（经ProjectileLibrary解析；0=无模板全内联）。
        /// 命中载荷来自模板；无模板时为空载荷（纯撞击弹）</summary>
        public uint projectileId;

        /// <summary>SpawnBullet/ControlBullet共用：弹句柄（同一caster内唯一即可）。
        /// SpawnBullet生成时注册；ControlBullet按此找到弹。0=不注册/不操控</summary>
        public int bulletLabel;

        /// <summary>SpawnBullet：速度覆盖（milli/tick，0=用模板值；无模板时必须>0）</summary>
        public int bulletSpeed;

        /// <summary>SpawnBullet：命中半径覆盖（milli，0=用模板值）</summary>
        public int bulletRadius;

        /// <summary>SpawnBullet：最大射程覆盖（milli，0=用模板值）</summary>
        public int bulletRange;

        /// <summary>ControlBullet：操控模式（BulletCtrlMode数值直转：0=追踪target/1=固定向量x,y,z）</summary>
        public int bulletCtrlMode;

        /// <summary>TriggerFx：特效库键（表现层BattleFxLibrary按此查找特效，短键非路径）。
        /// 空串=哑轨道（Validate拦截）；逻辑层仅透传该字符串，不解析其内容</summary>
        public string fxId = "";

        /// <summary>TriggerFx：特效位置用上下文弹着点（命中=接触点、超程=落点，照shapeCenterMode=ImpactPoint语义）。
        /// 无弹着点上下文（技能直放等）回退origin角色位置；持续型特效弹着点为固定点不跟随</summary>
        public bool fxUseImpact;
    }
}
