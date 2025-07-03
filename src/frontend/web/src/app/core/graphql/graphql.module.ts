import { provideApollo as provideApolloBase } from 'apollo-angular';
import { InMemoryCache } from '@apollo/client/core';

import { environment } from '../../../environments/environment';

export function provideApollo() {
  return provideApolloBase(() => {
    return {
      link: null, // Will be set by HttpLink
      uri: environment.apiUrl,
      cache: new InMemoryCache(),
      defaultOptions: {
        watchQuery: {
          errorPolicy: 'all',
        },
      },
    };
  });
}