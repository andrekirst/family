import { HttpLink } from 'apollo-angular/http';
import { APOLLO_OPTIONS } from 'apollo-angular';
import { ApolloClientOptions, InMemoryCache } from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';

import { environment } from '../../../environments/environment';

export function createApollo(httpLink: HttpLink): ApolloClientOptions<unknown> {
  const basic = httpLink.create({
    uri: environment.apiUrl,
  });

  const authLink = setContext(() => {
    const token = localStorage.getItem('accessToken');
    
    return {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    };
  });

  return {
    link: authLink.concat(basic),
    cache: new InMemoryCache(),
    defaultOptions: {
      watchQuery: {
        errorPolicy: 'all',
      },
    },
  };
}

export const apolloProvider = {
  provide: APOLLO_OPTIONS,
  useFactory: createApollo,
  deps: [HttpLink],
};