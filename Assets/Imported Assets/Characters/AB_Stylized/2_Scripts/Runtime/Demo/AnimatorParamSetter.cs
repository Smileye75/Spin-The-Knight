using UnityEngine;

namespace AnkleBreaker.Demo
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorParamSetter : MonoBehaviour
    {
        private Animator _animator;

        [System.Serializable]
        public class AnimatorParam
        {
            public string paramName;
            public ParamType type;
            public bool boolValue;
            public float floatValue;
            public int intValue;

            [Header("Trigger loop")] public bool loopTrigger = true;
            public float triggerInterval = 3f;
            [HideInInspector] public float triggerTimer;

            public bool setOnStart = true;

            public enum ParamType
            {
                Bool,
                Float,
                Int,
                Trigger
            }
        }

        public AnimatorParam[] parameters;

        void Start()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            // Optional: set some parameters at Start
            foreach (var param in parameters)
            {
                if (param.setOnStart)
                    ApplyParameter(param);
            }
        }

        void Update()
        {
            foreach (var param in parameters)
            {
                if (param.type == AnimatorParam.ParamType.Trigger && param.loopTrigger)
                {
                    param.triggerTimer += Time.deltaTime;
                    if (param.triggerTimer >= param.triggerInterval)
                    {
                        _animator.SetTrigger(param.paramName);
                        param.triggerTimer = 0f;
                    }
                }
            }
        }

        public void ApplyParameter(string name)
        {
            var p = System.Array.Find(parameters, x => x.paramName == name);
            if (p != null)
                ApplyParameter(p);
            else
                Debug.LogWarning($"Parameter '{name}' not found in list.");
        }

        public void ApplyParameter(AnimatorParam param)
        {
            switch (param.type)
            {
                case AnimatorParam.ParamType.Bool:
                    _animator.SetBool(param.paramName, param.boolValue);
                    break;
                case AnimatorParam.ParamType.Float:
                    _animator.SetFloat(param.paramName, param.floatValue);
                    break;
                case AnimatorParam.ParamType.Int:
                    _animator.SetInteger(param.paramName, param.intValue);
                    break;
                case AnimatorParam.ParamType.Trigger:
                    _animator.SetTrigger(param.paramName);
                    break;
            }
        }
    }
}