using System;
using System.ComponentModel;
using System.Linq.Expressions;
namespace GalaSoft.MvvmLight;
public class ObservableObject : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	protected void RaisePropertyChanged<T>(Expression<Func<T>> e)
	{
		if (e.Body is MemberExpression m) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(m.Member.Name));
	}
}
public class ViewModelBase : ObservableObject { }
