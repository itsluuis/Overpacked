using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Gameplay
{
    public class OperatorDrawerButton : MonoBehaviour
    {
        public PropositionalOperator Operator;
        public AssemblyWorkbenchManager Workbench;

        private void OnMouseDown()
        {
            if (Workbench == null)
            {
                Workbench = AssemblyWorkbenchManager.Instance;
            }

            if (Workbench != null)
            {
                Workbench.SelectOperator(Operator);
            }
        }
    }

    public class OperatorSlotTrigger : MonoBehaviour
    {
        public AssemblyWorkbenchManager Workbench;

        private void OnMouseDown()
        {
            if (Workbench == null)
            {
                Workbench = AssemblyWorkbenchManager.Instance;
            }

            if (Workbench != null)
            {
                Workbench.ToggleOperatorDrawer();
            }
        }
    }
}
