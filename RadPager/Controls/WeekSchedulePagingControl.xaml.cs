﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace RadPager.Controls
{
    public partial class WeekSchedulePagingControl : UserControl, INotifyPropertyChanged
    {
        private const int MaxPages = 3;

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(LoadChunk), typeof(UserControl), new PropertyMetadata(null, OnUpdateCurrentPage));
        public LoadChunk CurrentPage
        {
            get { return (LoadChunk)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }
        private static void OnUpdateCurrentPage(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as WeekSchedulePagingControl;
            if (ctrl == null || e.NewValue == e.OldValue) return;

            var currentPage = e.NewValue as LoadChunk;
            if (currentPage != null)
            {
                ctrl.UpdateCurrentPage(currentPage);
            }
        }

        private void UpdateCurrentPage(LoadChunk currentPage)
        {
            if (Pages == null) return;
            var currentPageIndex = Pages.IndexOf(currentPage);
            if (!VisiblePages.Contains(currentPage))
            {
                var lastPageIndex = Pages.Count() - 1;
                if (Pages.Count <= MaxPages)
                {
                    VisiblePages = new ObservableCollection<LoadChunk>(Pages.ToList());
                }
                if ((lastPageIndex-currentPageIndex) < MaxPages)
                {
                    var firstindex = Pages.Count - MaxPages;
                    VisiblePages = new ObservableCollection<LoadChunk>(Pages.Skip(firstindex).Take(MaxPages).ToList());
                }
                else
                {
                    VisiblePages = new ObservableCollection<LoadChunk>(Pages.Skip(currentPageIndex).Take(MaxPages).ToList());    
                }
                
            }
            
            foreach (var visiblePage in VisiblePages)
            {
                visiblePage.IsSelected = (visiblePage == currentPage);
            }

            if (currentPage != null && currentPage != CurrentPage)
            {
                CurrentPage = currentPage;
                RaisePropertyChanged(() => CurrentPageNo);
            }
            UpdateButtons();
            
        }

        public static readonly DependencyProperty PagesProperty = DependencyProperty.Register("Pages", typeof(ObservableCollection<LoadChunk>), typeof(UserControl), new PropertyMetadata(null, OnUpdatePages));
        private static void OnUpdatePages(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as WeekSchedulePagingControl;
            if (ctrl == null)
                throw new Exception("Pages context missing in WeekSchedulePagingControl!");

            ctrl.UpdatePages(e.NewValue as ObservableCollection<LoadChunk>);
        }


        private void UpdatePages(ObservableCollection<LoadChunk> pages)
        {
            if (pages == null || pages.Count == 1)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
                VisiblePages = new ObservableCollection<LoadChunk>(pages.Take(Math.Min(pages.Count, MaxPages)).ToList());
                CurrentPage = VisiblePages[0];
                CurrentPage.IsSelected = true;    
                RaisePropertyChanged(() => CurrentPageNo);
            }
        }

        public int CurrentPageNo { get { return Pages.IndexOf(CurrentPage) + 1; } }

        public ObservableCollection<LoadChunk> Pages
        {
            get { return (ObservableCollection<LoadChunk>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public ObservableCollection<LoadChunk> VisiblePages
        {
            get { return _visiblePages; }
            set { _visiblePages = value; RaisePropertyChanged(() => VisiblePages); }
        }

        // * Dep-props
        // CurrentPage (kopplad till CurrentPage (är en chunk))
        // Pages (samling med chunks)

        // * Kommandon
        // Next page, prev, first, last, set page(textbox) -> sätter CurrentPage + uppdatera intern pages samling för att bara visa de flikar som får plats
        // setpage kan vi avvakta med, gör readonly för tillfället... annars får användaren ange ett pe och så letar vi fram den page som innehåller det

        // * Utritning (anpassning i canvas)
        // Koppla in CurrentPage som dependency property, rita om vid förändring (håll lokal variabel som visar vad som är redan är utritat)


        public WeekSchedulePagingControl()
        {
            InitializeComponent();            
        }


        public bool IsNextEnabled
        {
            get { return _isNextEnabled; }
            set { _isNextEnabled = value; RaisePropertyChanged(() => IsNextEnabled); }
        }

        public bool IsPreviousEnabled
        {
            get { return _isPreviousEnabled; }
            set { _isPreviousEnabled = value; RaisePropertyChanged(() => IsPreviousEnabled); }
        }

        public bool IsFirstEnabled
        {
            get { return _isFirstEnabled; }
            set { _isFirstEnabled = value; RaisePropertyChanged(() => IsFirstEnabled); }
        }

        public bool IsLastEnabled
        {
            get { return _isLastEnabled; }
            set { _isLastEnabled = value; RaisePropertyChanged(() => IsLastEnabled); }
        }

        private void UpdateButtons()
        {            
            IsLastEnabled =  IsNextEnabled = (Pages != null && CurrentPage != null && CurrentPage != Pages.Last());
            IsFirstEnabled = IsPreviousEnabled = (Pages != null && CurrentPage != null && CurrentPage != Pages.First());
        }

        #region Button events
        private void PageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as RadRadioButton;
            if (ctrl != null)
            {
                var page = ctrl.CommandParameter as LoadChunk;
                if (page != null)
                {
                    UpdateCurrentPage(page);
                }
            }

        }

        // Workaround to fix initial IsSelected is not updating the button state
        private void PageButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as RadRadioButton;
            if (ctrl != null)
            {
                var page = ctrl.DataContext as LoadChunk;
                if (page != null)
                {
                    if (page.IsSelected)
                    {
                        ctrl.IsChecked = true;
                    }
                }
            }
        }

        private void MoveToNextPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var indexCurrent = Pages.IndexOf(CurrentPage);
            var next = Pages[indexCurrent + 1];
            UpdateCurrentPage(next);
        }

        private void MoveToPreviousPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var indexCurrent = Pages.IndexOf(CurrentPage);
            var previous = Pages[indexCurrent - 1];
            UpdateCurrentPage(previous);
        }

        private void MoveToFirstPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCurrentPage(Pages.First());
        }

        private void MoveToLastPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCurrentPage(Pages.Last());
        }
        #endregion

 
        #region INotify Implementation
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                RaisePropertyChanged(name);
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ExtractPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);
        }

        protected string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("Property not a member", "propertyExpression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("Not a propery", "propertyExpression");
            }

            var getMethod = property.GetGetMethod(true);

            if (getMethod == null)
            {
                // this shouldn't happen - the expression would reject the property before reaching this far
                throw new ArgumentException("No getter", "propertyExpression");
            }

            if (getMethod.IsStatic)
            {
                throw new ArgumentException("Static property", "propertyExpression");
            }

            return memberExpression.Member.Name;
        }


        /// <summary>
        ///     Internal list of errors
        /// </summary>
        private readonly Dictionary<string, IEnumerable<string>> _errors = new Dictionary<string, IEnumerable<string>>();

        private ObservableCollection<LoadChunk> _visiblePages;
        private bool _isNextEnabled;
        private bool _isPreviousEnabled;
        private bool _isFirstEnabled;
        private bool _isLastEnabled;

        /// <summary>
        ///     Collction of errors changed
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;


        /// <summary>
        ///     True if errors exist
        /// </summary>
        public bool HasErrors
        {
            get { return _errors.Any(); }
        }

        /// <summary>
        ///     Get errors for a property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns></returns>
        protected virtual IEnumerable GetErrors(string propertyName)
        {
            IEnumerable<string> error;
            return _errors.TryGetValue(propertyName ?? string.Empty, out error) ? error : null;
        }

        /// <summary>
        ///     Set an error for a property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="error">The error</param>
        public virtual void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            SetErrors(propertyName, new List<string> { error });
        }

        /// <summary>
        ///     Overload for expression
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpresssion">An expression that points to the property</param>
        /// <param name="error">The error</param>
        protected virtual void SetError<T>(Expression<Func<T>> propertyExpresssion, string error)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            SetError(propertyName, error);
        }

        /// <summary>
        ///     Clears the errors
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void ClearErrors(string propertyName)
        {
            SetErrors(propertyName, new List<string>());
        }

        /// <summary>
        ///     Clear all errors for a property
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpresssion">The expression that points to the property</param>
        protected virtual void ClearErrors<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            ClearErrors(propertyName);
        }

        /// <summary>
        ///     Set errors for a property
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="propertyErrors">The collection of errors</param>
        protected virtual void SetErrors<T>(Expression<Func<T>> propertyExpresssion, IEnumerable<string> propertyErrors)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            SetErrors(propertyName, propertyErrors);
        }

        /// <summary>
        ///     Set errors for a property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyErrors">The collection of errors</param>
        protected virtual void SetErrors(string propertyName, IEnumerable<string> propertyErrors)
        {
            if (propertyErrors.Any(error => error == null))
            {
                throw new ArgumentException("No null errors", "propertyErrors");
            }

            var propertyNameKey = propertyName ?? string.Empty;

            IEnumerable<string> currentPropertyErrors;
            if (_errors.TryGetValue(propertyNameKey, out currentPropertyErrors))
            {
                if (!_AreErrorCollectionsEqual(currentPropertyErrors, propertyErrors))
                {
                    if (propertyErrors.Any())
                    {
                        _errors[propertyNameKey] = propertyErrors;
                    }
                    else
                    {
                        _errors.Remove(propertyNameKey);
                    }

                    RaiseErrorsChanged(propertyNameKey);
                }
            }
            else
            {
                if (propertyErrors.Any())
                {
                    _errors[propertyNameKey] = propertyErrors;
                    RaiseErrorsChanged(propertyNameKey);
                }
            }
        }

        /// <summary>
        ///     Raises this object's ErrorsChangedChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has new errors.</param>
        protected virtual void RaiseErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            if (handler != null)
            {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Raises this object's ErrorsChangedChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has errors</typeparam>
        /// <param name="propertyExpresssion">A Lambda expression representing the property that has new errors.</param>
        protected virtual void RaiseErrorsChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = ExtractPropertyName(propertyExpresssion);
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        ///     Compares error collections
        /// </summary>
        /// <param name="propertyErrors">The property errors</param>
        /// <param name="currentPropertyErrors">The current</param>
        /// <returns>True if there are/aren't equal</returns>
        private static bool _AreErrorCollectionsEqual(IEnumerable<string> propertyErrors,
                                                      IEnumerable<string> currentPropertyErrors)
        {
            var equals = currentPropertyErrors.Zip(propertyErrors, (current, newError) => current == newError);
            return propertyErrors.Count() == currentPropertyErrors.Count() && equals.All(b => b);
        }
        #endregion
    }

    //TODO Remove 
    public class LoadChunk : BaseViewModel
    {
        public LoadChunk()
        {
            IsLoaded = true; // TODO:
        }

        private bool _isSelected;
        private bool _isLoaded;
        public int StartPlanningUnitId { get; set; }
        public string StartPlanningUnitNumber { get; set; }
        public int EndPlanningUnitId { get; set; }
        public string EndPlanningUnitNumber { get; set; }
        public List<int> PlanningUnitIds { get; set; }
        public string DisplayText { get { return string.Format("{0}-{1}", StartPlanningUnitNumber, EndPlanningUnitNumber); } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged(() => IsSelected); }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; RaisePropertyChanged(() => IsLoaded); }
        }
    }
}
