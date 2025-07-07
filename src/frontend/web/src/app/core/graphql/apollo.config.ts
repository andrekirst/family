import { Injectable } from '@angular/core';
import { HttpLink } from 'apollo-angular/http';
import { InMemoryCache, ApolloLink } from '@apollo/client/core';
import { setContext } from '@apollo/client/link/context';
import { onError } from '@apollo/client/link/error';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApolloConfigService {
  
  constructor(private httpLink: HttpLink) {}

  createApollo() {
    // HTTP Link
    const http = this.httpLink.create({
      uri: environment.graphqlEndpoint || '/graphql'
    });

    // Auth Link - Add JWT token to headers
    const auth = setContext((operation, context) => {
      const token = localStorage.getItem('accessToken');
      
      if (token) {
        return {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        };
      }
      
      return {
        headers: {
          'Content-Type': 'application/json'
        }
      };
    });

    // Error Link - Handle GraphQL and network errors
    const error = onError(({ graphQLErrors, networkError, operation, forward }) => {
      if (graphQLErrors) {
        graphQLErrors.forEach(({ message, locations, path, extensions }) => {
          console.error(
            `GraphQL error: Message: ${message}, Location: ${locations}, Path: ${path}`
          );
          
          // Handle authentication errors
          if (extensions?.['code'] === 'UNAUTHENTICATED') {
            // Clear token and redirect to login
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            window.location.href = '/login';
          }
        });
      }

      if (networkError) {
        console.error(`Network error: ${networkError}`);
        
        // Handle network errors (e.g., server down, no internet)
        if ('statusCode' in networkError && networkError.statusCode === 401) {
          // Unauthorized - clear tokens and redirect
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          window.location.href = '/login';
        }
      }
    });

    // Create Apollo Link chain
    const link = ApolloLink.from([
      error,
      auth,
      http
    ]);

    // Create In-Memory Cache
    const cache = new InMemoryCache({
      typePolicies: {
        Query: {
          fields: {
            // Cache policies for family-related queries
            getMyFamily: {
              merge: false // Always replace cached data
            },
            getFirstTimeUserInfo: {
              merge: false
            }
          }
        }
      }
    });

    return {
      link,
      cache,
      defaultOptions: {
        watchQuery: {
          errorPolicy: 'all' as const,
          fetchPolicy: 'cache-and-network' as const
        },
        query: {
          errorPolicy: 'all' as const,
          fetchPolicy: 'cache-first' as const
        },
        mutate: {
          errorPolicy: 'all' as const
        }
      }
    };
  }
}