import { NgModule } from '@angular/core';
import { APOLLO_OPTIONS } from 'apollo-angular';
import { ApolloClientOptions, InMemoryCache, createHttpLink } from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';

const uri = 'http://localhost:8081/graphql'; // Family API GraphQL endpoint

export function apolloOptionsFactory(): ApolloClientOptions<any> {
  const httpLink = createHttpLink({
    uri,
  });

  const authLink = setContext((_, { headers }) => {
    // Get authentication token from localStorage
    const token = localStorage.getItem('accessToken');
    
    return {
      headers: {
        ...headers,
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    };
  });

  return {
    link: authLink.concat(httpLink),
    cache: new InMemoryCache(),
    defaultOptions: {
      watchQuery: {
        errorPolicy: 'all',
      },
    },
  };
}

@NgModule({
  providers: [
    {
      provide: APOLLO_OPTIONS,
      useFactory: apolloOptionsFactory,
    },
  ],
})
export class GraphQLModule {}