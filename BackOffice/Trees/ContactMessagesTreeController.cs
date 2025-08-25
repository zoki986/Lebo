//using Microsoft.AspNetCore.Mvc;
//using Umbraco.Cms.Core;
//using Umbraco.Cms.Core.Events;
//using Umbraco.Cms.Core.Services;
//using Umbraco.Cms.Core.Trees;
//using Umbraco.Cms.Web.BackOffice.Trees;
//using Umbraco.Cms.Web.Common.Attributes;
//using Umbraco.Cms.Web.Common.ModelBinders;

//namespace Lebo.BackOffice.Trees
//{
//    [Tree("contactMessages", "dashboard", TreeTitle = "Dashboard", SortOrder = 1)]
//    [PluginController("contactMessages")]
//    public class ContactMessagesTreeController : TreeController
//    {
//        public ContactMessagesTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
//        {
//        }

//        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
//        {
//            return new MenuItemCollection(new Umbraco.Cms.Core.Actions.ActionCollection(() => []));
//        }

//        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
//        {
//            var nodes = new TreeNodeCollection();

//            if (id == Constants.System.Root.ToInvariantString())
//            {
//                var node = CreateTreeNode(
//                    "messages", id, queryStrings,
//                    "Messages", "icon-list", false,
//                    "contactMessages/contactMessagesDashboard");

//                nodes.Add(node);
//            }

//            return nodes;
//        }
//    }
//}
