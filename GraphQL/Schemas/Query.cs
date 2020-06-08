using System.Linq;
using api.GraphQL.Types;
using api.Models;
using GraphQL.Types;

namespace api.GraphQL.Schemas
{
    class Query : ObjectGraphType<object>
    {
        public Query(Db db)
        {
            Name = "Query";
            Field<ListGraphType<UserType>>(
                "users",
                resolve: context => db.Users.ToList()
            );
        }
    }
}