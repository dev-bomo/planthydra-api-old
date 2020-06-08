using GraphQL;
using GraphQL.Types;

namespace api.GraphQL.Schemas
{
    class MeridiaSchema : Schema
    {
        public MeridiaSchema(Query query, IDependencyResolver resolver)
        {
            Query = query;
            DependencyResolver = resolver;
        }
    }
}