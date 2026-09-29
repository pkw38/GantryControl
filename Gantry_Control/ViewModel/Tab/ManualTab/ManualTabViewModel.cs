using CommunityToolkit.Mvvm.ComponentModel;

namespace Gantry_Control.ViewModel.Tab.ManualTab
{
    public abstract class ManualTabViewModel : ObservableObject
    {
        public abstract string Header { get; }

        // 탭/화면 전환, 창 비활성화 시 호출. 장비를 움직이는 탭은 재정의해서 정지 처리
        public virtual void StopMotion() { }
    }
}
