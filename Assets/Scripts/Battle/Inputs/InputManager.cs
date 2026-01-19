using System.Collections.Generic;
using Tools.Singletons;

namespace Battle.Inputs
{
    public class InputManager : Singleton<InputManager>
    {
        private List<BaseInput> inputs = new List<BaseInput>();
        private Dictionary<string,BaseInput> inputDic = new Dictionary<string, BaseInput>();
        
        public void RegisterInputKey(BaseInput input)
        {
            inputs.Add(input);
        }

        public void RegisterKeyBoardKey()
        {
            
        }

        public void Update(float deltaTime)
        {
            
        }
    }
}