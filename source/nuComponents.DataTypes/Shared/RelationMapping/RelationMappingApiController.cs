﻿namespace nuComponents.DataTypes.Shared.RelationMapping
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Core.Models;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;

    [PluginController("nuComponents")]
    public class RelationMappingApiController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<object> GetRelationTypes()
        {
            return ApplicationContext.Services.RelationService.GetAllRelationTypes()
                        .OrderBy(x => x.Name)
                        .Select(x => new
                        {
                            key = x.Alias,
                            label = x.Name,
                            bidirectional = x.IsBidirectional                           
                        });
        }
       
        [HttpGet]
        public IEnumerable<int> GetRelatedIds([FromUri] int contextId, [FromUri] string relationTypeAlias, [FromUri] string relationScopeIdentifier)
        {
            // Get child Ids for current contextId and relationType            
            IRelationType relationType = ApplicationContext.Services.RelationService.GetRelationTypeByAlias(relationTypeAlias);

            if (relationType != null)
            {
                return ApplicationContext.Services.RelationService.GetAllRelationsByRelationType(relationType.Id)
                                .Where(x => x.ChildId == contextId && (string.IsNullOrWhiteSpace(relationScopeIdentifier) || x.Comment == relationScopeIdentifier))
                                .Select(x => x.ParentId);
            }

            return null;
        }        

        [HttpPost]
        public void UpdateRelationMapping([FromUri] int contextId, [FromUri] string relationTypeAlias, [FromUri] string relationScopeIdentifier, [FromBody] dynamic data)
        {
            IRelationType relationType = ApplicationContext.Services.RelationService.GetRelationTypeByAlias(relationTypeAlias);

            if (relationType != null)
            {
                RelationMapping.DeleteRelations(relationType, contextId, relationScopeIdentifier);
                
                foreach(int pickedId in ((JArray)data).Select(x => x.Value<int>()))
                {                    
                    RelationMapping.CreateRelation(relationType, contextId, pickedId, relationScopeIdentifier);
                }
            }
        }

    }
}
