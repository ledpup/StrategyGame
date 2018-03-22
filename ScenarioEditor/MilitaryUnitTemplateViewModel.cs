using GameModel;

namespace ScenarioEditor
{
    internal class MilitaryUnitTemplateViewModel : BaseViewModel
    {
        private MilitaryUnitTemplate _militaryUnitTemplate;

        public MilitaryUnitTemplateViewModel(MilitaryUnitTemplate militaryUnitTemplate)
        {
            _militaryUnitTemplate = militaryUnitTemplate;
        }

        public string Name
        {
            get { return _militaryUnitTemplate.Name; }
            set
            {
                if (value == _militaryUnitTemplate.Name)
                    return;

                _militaryUnitTemplate.Name = value;
                RaisePropertyChanged();
            }
        }

        public MovementType MovementType
        {
            get { return _militaryUnitTemplate.MovementType; }
            set
            {
                if (value == _militaryUnitTemplate.MovementType)
                    return;

                _militaryUnitTemplate.MovementType = value;
                RaisePropertyChanged();
            }
        }

        public bool UsesRoads
        {
            get { return _militaryUnitTemplate.UsesRoads; }
            set
            {
                if (value == _militaryUnitTemplate.UsesRoads)
                    return;

                _militaryUnitTemplate.UsesRoads = value;
                RaisePropertyChanged();
            }
        }

        public int MovementPoints
        {
            get { return _militaryUnitTemplate.MovementPoints; }
            set
            {
                if (value == _militaryUnitTemplate.MovementPoints)
                    return;

                _militaryUnitTemplate.MovementPoints = value;
                RaisePropertyChanged();
            }
        }

        public bool CanTransport
        {
            get { return _militaryUnitTemplate.CanTransport; }
            set
            {
                if (value == _militaryUnitTemplate.CanTransport)
                    return;

                _militaryUnitTemplate.CanTransport = value;
                RaisePropertyChanged();
            }
        }
    }
}