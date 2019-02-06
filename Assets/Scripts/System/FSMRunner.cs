﻿using System;

namespace Assets.Scripts.System
{
    public class FSMRunner : ILateUpdateable
    {
        private static FSMRunner _instance;
        public static FSMRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FSMRunner();
                }

                return _instance;
            }
        }

        public float[] Timers { get; set; }

        public FSM FSM;
        private FSMActionDelegator _actionDelegator;
        
        private FSMRunner()
        {
            Timers = new float[10];
            _actionDelegator = new FSMActionDelegator();
            UpdateManager.Instance.AddLateUpdateable(this);
        }

        public void Destroy()
        {
            UpdateManager.Instance.RemoveLateUpdateable(this);
        }

        public void LateUpdate()
        {
            if (FSM == null)
            {
                return;
            }

            int currentMachineIndex = 0;
            while (currentMachineIndex < FSM.StackMachines.Length)
            {
                StackMachine machine = FSM.StackMachines[currentMachineIndex];
                if (machine.Halted || (Step(machine) == StepResult.DoNextMachine))
                {
                    currentMachineIndex++;
                }
            }
        }
        
        private StepResult Step(StackMachine machine)
        {
            ByteCode byteCode = FSM.ByteCode[machine.IP++];
            switch (byteCode.OpCode)
            {
                case OpCode.PUSH:
                    machine.Stack.Push(new IntRef(byteCode.Value));
                    break;
                case OpCode.ARGPUSH_S:
                    IntRef sVal = machine.Stack[byteCode.Value - 1];

                    machine.ArgumentQueue.Enqueue(sVal);
                    break;
                case OpCode.ARGPUSH_B:
                    int idx = machine.Constants.Length + (byteCode.Value + 1);
                    IntRef bVal = machine.Constants[idx];
                    machine.ArgumentQueue.Enqueue(bVal);
                    break;
                case OpCode.ADJUST:
                    int addToSP = byteCode.Value;

                    if (addToSP < 1)
                        throw new NotImplementedException("What to do when adjusting 0 or negative values?");

                    for (int i = 0; i < addToSP; i++)
                    {
                        machine.Stack.Push(new IntRef(0));
                    }
                    break;
                case OpCode.DROP:
                    int subFromSP = byteCode.Value;

                    if (subFromSP < 0)
                        throw new NotImplementedException("Expecting positive values");

                    for (int i = 0; i < subFromSP; i++)
                    {
                        machine.Stack.Pop();
                    }
                    break;
                case OpCode.JMP:
                    machine.IP = (uint)byteCode.Value;
                    break;
                case OpCode.JZ:
                    if (machine.ResultReg == 0)
                        machine.IP = (uint)byteCode.Value;
                    break;
                case OpCode.JMP_I:
                    machine.IP = (uint)byteCode.Value;
                    return StepResult.DoNextMachine;
                case OpCode.RST:
                    machine.Halted = true;
                    return StepResult.DoNextMachine;
                case OpCode.ACTION:
                    string actionName = FSM.ActionTable[byteCode.Value];
                    machine.ResultReg = _actionDelegator.DoAction(actionName, machine, this);
                    machine.ArgumentQueue.Clear();
                    break;
                case OpCode.NEG:
                    if (machine.ResultReg == 1)
                    {
                        machine.ResultReg = 0;
                    }
                    else
                    {
                        machine.ResultReg = 1;
                    }
                    break;
                default:
                    throw new NotImplementedException("Unimplemented bytecode " + byteCode.OpCode);
            }

            return StepResult.NotDone;
        }

        private enum StepResult
        {
            NotDone,
            DoNextMachine
        }
    }
}
