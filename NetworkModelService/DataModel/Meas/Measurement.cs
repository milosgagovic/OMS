using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Meas
{
    public class Measurement : IdentifiedObject
    {

        /// Specifies the type of measurement.  For example, this specifies if the measurement represents an indoor temperature, outdoor temperature, bus voltage, line flow, etc.
        private string measurementType;

        /// The unit of measure of the measured quantity.
        private UnitSymbol unitSymbol;

        private DirectionType direction;

        public Measurement(long globalId) :
                base(globalId)
        {
        }

        public string Measurementtype
        {
            get
            {
                return this.measurementType;
            }
            set
            {
                this.measurementType = value;
            }
        }

        public UnitSymbol Unitsymbol
        {
            get
            {
                return this.unitSymbol;
            }
            set
            {
                this.unitSymbol = value;
            }
        }

        public DirectionType Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;
            }
        }

        
        public override bool IsReferenced
        {
            get
            {
                return
                base.IsReferenced;
            }
        }

        public override bool Equals(object obj)
        {
            if ((true && base.Equals(obj)))
            {
                Measurement x = (Measurement)obj;
                return (
                (x.measurementType == this.measurementType) &&
                (x.unitSymbol == this.unitSymbol) &&
                (x.direction == this.direction));
               
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_TYPE:
                    property.SetValue(measurementType);
                    break;
                case ModelCode.MEASUREMENT_UNITSYMB:
                    property.SetValue((short)unitSymbol);
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    property.SetValue((short)direction);
                    break;
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.MEASUREMENT_TYPE:
                case ModelCode.MEASUREMENT_UNITSYMB:
                case ModelCode.MEASUREMENT_DIRECTION:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_TYPE:
                    measurementType = property.AsString();
                    break;
                case ModelCode.MEASUREMENT_UNITSYMB:
                    unitSymbol = (UnitSymbol)property.AsEnum();
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    direction = (DirectionType)property.AsEnum();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
           
            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
               
                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }
    }
}
