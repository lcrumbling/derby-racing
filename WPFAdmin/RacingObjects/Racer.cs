using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GSRacing.RacingObjects
{
    public class Racer : ObservableObject
    {
        private Guid _racerID;
        public Guid RacerID
        {
            get { return _racerID; }
            set
            {
                this.Set(ref this._racerID, value);
            }
        }

        private Guid _eventID;
        public Guid EventID
        {
            get { return _eventID; }
            set
            {
                this.Set(ref this._eventID, value);
            }
        }

        private int _regNumber;
        public int RegNumber
        {
            get { return _regNumber; }
            set
            {
                this.Set(ref this._regNumber, value);
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                this.Set(ref this._firstName, value);
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                this.Set(ref this._lastName, value);
            }
        }
        public string FullName
        {
            get { return string.Format("{0} {1}", this.FirstName, this.LastName);  }
        }

    }
}
