using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HardHatCore.ContractorSystem.Contexts.Types;
using HardHatCore.ContractorSystem.Contractors.ContractorCommTypes;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Services
{
    public partial class ContractSystemFactory
    {
        #region Contexts
        
        public static async Task<IEnumerable<EventContext>> GetSubableEvents()
        {
            var Contexts = await Contractor_Database.GetItemsOfType<EventContext>(typeof(EventContext));
            return Contexts;
        }

        public static async Task<IContextBase> GetContextById(string id)
        {
            var Context = await Contractor_Database.GetItemById<EventContext>(id);
            return Context;
        }

        public static async Task<EventContext> CreateSubEvent(string name,string desc , Dictionary<string,object> Properties)
        {
            return new EventContext()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = desc,
                Properties = Properties,
                EventSubs = new List<IContractor>()
            };
        }

        public static async Task<Tuple<bool,string>> RegisterForEvent(IContractor contractor, string EventId)
        {
            EventContext eventToSub = (EventContext)await GetContextById(EventId);
            if (eventToSub == null)
            {
                return new Tuple<bool, string>(false, "Event does not exist");
            }
            else
            {
                _ = eventToSub.EventSubs.Append(contractor);
                await Contractor_Database.UpdateItem(eventToSub,typeof(EventContext));
                return new Tuple<bool, string>(true, $"{contractor.Name} subbed to event {eventToSub.Name}");
            }
        }



        #endregion


        #region Contractors
        /// <summary>
        /// Create an instance of a contractor with a unique Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="contractorCommDetials"></param>
        /// <returns></returns>
        public static async Task<T> CreateContractor<T>(string name, string? description, ICommunicationDetails contractorCommDetials) where T : IContractor
        {
            //create the contractor
            T _contractor = (T)Activator.CreateInstance(typeof(T));
            _contractor.Description = description;
            _contractor.Name = name;
            _contractor.Id = Guid.NewGuid().ToString();
            _contractor.CommunicationDetails = contractorCommDetials;
            return _contractor;

        }

        /// <summary>
        /// Get all contractors
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<IContractor>> GetContractors()
        {
            return await Contractor_Database.GetItemsOfType<IContractor>(typeof(IContractor));
        }


        /// <summary>
        /// Return all contractors of a specific type
        /// </summary>
        public static async Task<IEnumerable<T>> GetAllContractorsOfType<T>()
        {
            return await Contractor_Database.GetItemsOfType<T>(typeof(IContractor));
        }


        /// <summary>
        /// Get a contractor by the contractor Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static async Task<IContractor?> GetContractorById(string Id)
        {
            List<IContractor> _contractors = await Contractor_Database.GetItemsOfType<IContractor>(typeof(IContractor));
            return _contractors.FirstOrDefault(x => x.Id == Id);
        }

        #endregion

    }
}
