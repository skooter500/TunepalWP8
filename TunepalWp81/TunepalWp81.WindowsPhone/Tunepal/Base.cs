using System.ComponentModel;

namespace Tunepal.Core {
	public class Base : INotifyPropertyChanged {
		#region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(string name) {
            if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
	}
}
