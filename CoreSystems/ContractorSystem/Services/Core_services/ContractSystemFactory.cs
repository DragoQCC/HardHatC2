using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using HardHatCore.ContractorSystem.Contexts.Types;
using HardHatCore.ContractorSystem.Contractors.ContractorCommTypes;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Services
{
    public partial class ContractSystemFactory
    {
        #region Contexts

        public async Task<IContextBase?> GetContextById(string id)
        {
            var Context = ContractorSystem_Cache.Contexts.FirstOrDefault(x => x.Id.Equals(id));
            return Context;
        }

        public async Task<IEnumerable<IContextBase>> GetContextsBySource(string sourceLocation)
        {
            var context = ContractorSystem_Cache.Contexts.Where(x => x.SourceLocation.Equals(sourceLocation, StringComparison.CurrentCultureIgnoreCase));
            return context;
        }

        public async Task<IEnumerable<EventContext>> GetsubscribableEvents()
        {
            var Contexts = await Contractor_Database.GetItemsOfType<EventContext>(typeof(EventContext));
            return Contexts;
        }

        public async Task<IEnumerable<TaskContext>> GetPublishedTasks()
        {
            var Contexts = await Contractor_Database.GetItemsOfType<TaskContext>(typeof(TaskContext));
            return Contexts;
        }

        public async Task<IEnumerable<DataChangeContext>> GetSubscribableDataChangeEvents()
        {
            var contexts = await Contractor_Database.GetItemsOfType<DataChangeContext>(typeof(DataChangeContext));
            return contexts;
        }
        
        public async Task<IContextBase?> CreateContext(string name, string desc, string sourceloc, string contextType)
        {
            switch (contextType)
            {
                case "Event":
                    return await CreateContext<EventContext>(name, desc, sourceloc);
                    break;
                case "DataChange":
                    return await CreateContext<DataChangeContext>(name, desc, sourceloc);
                    break;
                case "Task":
                    return await CreateContext<TaskContext>(name, desc, sourceloc);
                    break;
                default:
                    break;
            }
            return null;
        }

        internal async Task<T> CreateContext<T>(string name, string desc, string sourceloc) where T : IContextBase
        {
            //use the Activator to create a new instance of the context and pass in the parameters at creation
            T _context = (T)Activator.CreateInstance(typeof(T), name, desc, sourceloc);
            return _context;

        }

        public async Task<Tuple<bool,string>> RegisterForEvent(IContractor contractor, string EventId)
        {
            EventContext eventToSub = (EventContext)await GetContextById(EventId);
            if (eventToSub == null)
            {
                return new Tuple<bool, string>(false, "Event does not exist");
            }
            else
            {
                if (eventToSub.Subscribers.Contains(contractor) is false)
                {
                    eventToSub.Subscribers.Add(contractor);
                    contractor.RelatedContexts.Add(eventToSub);
                }
                await Contractor_Database.UpdateItem(eventToSub,typeof(EventContext));
                return new Tuple<bool, string>(true, $"{contractor.Name} subbed to event {eventToSub.Name}");
            }
        }

        public async Task<Tuple<bool,string>> RegisterForDataChangeEvent(string ContractorId, string eventId)
        {
            IContractor? contractor = await GetContractorById(ContractorId);
            if (contractor == null)
            {
                return new Tuple<bool, string>(false, "Contractor does not exist");
            }
            DataChangeContext? eventToSub = (DataChangeContext)await GetContextById(eventId);
            if (eventToSub == null)
            {
                return new Tuple<bool, string>(false, "event not found");
            }
            
            eventToSub.Subscribers.Add(contractor);
            contractor.RelatedContexts.Add(eventToSub);
            await Contractor_Database.UpdateItem(eventToSub,typeof(DataChangeContext));
            return new Tuple<bool, string>(true, $"{contractor.Name} subbed to event {eventToSub.Name}");
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
        public async Task<T> CreateContractor<T>(string name, string? description, ICommunicationDetails contractorCommDetials) where T : IContractor
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
        public async Task<IEnumerable<IContractor>> GetContractors()
        {
            return await Contractor_Database.GetItemsOfType<IContractor>(typeof(IContractor));
        }


        /// <summary>
        /// Return all contractors of a specific type
        /// </summary>
        public async Task<IEnumerable<T>> GetAllContractorsOfType<T>()
        {
            return await Contractor_Database.GetItemsOfType<T>(typeof(IContractor));
        }


        /// <summary>
        /// Get a contractor by the contractor Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IContractor?> GetContractorById(string Id)
        {
            List<IContractor> _contractors = await Contractor_Database.GetItemsOfType<IContractor>(typeof(IContractor));
            return _contractors.FirstOrDefault(x => x.Id == Id);
        }

        #endregion

    }
}
