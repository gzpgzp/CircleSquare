using Battle.Character.CharacterMotor;

namespace Battle.Character
{
    public class Character2D : BaseCharacter
    {
        public CharacterMotor2D motor { get; protected set; }

        public override void Init(CharacterContext ctx)
        {
            base.Init(ctx);
            motor = new CharacterMotor2D();
            motor.InitMotor(GetComponent<MotorHelper>());
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            motor.Tick(dt);
        }
    }
}