import { Injectable } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Observable, map, catchError, of } from 'rxjs';

export interface FamilyInfo {
  id: string;
  name: string;
  ownerId: string;
  createdAt: string;
  members: FamilyMember[];
}

export interface FamilyMember {
  userId: string;
  role: string;
  joinedAt: string;
}

export interface CreateFamilyResult {
  success: boolean;
  family?: FamilyInfo;
  errorMessage?: string;
  validationErrors?: string[];
}

export interface FirstTimeUserInfo {
  isFirstTime: boolean;
  hasFamily: boolean;
  familyId?: string;
  familyName?: string;
}

// GraphQL Queries and Mutations
const GET_FIRST_TIME_USER_INFO = gql`
  query GetFirstTimeUserInfo {
    getFirstTimeUserInfo {
      isFirstTime
      hasFamily
      familyId
      familyName
    }
  }
`;

const IS_FIRST_TIME_USER = gql`
  query IsFirstTimeUser {
    isFirstTimeUser
  }
`;

const GET_MY_FAMILY = gql`
  query GetMyFamily {
    getMyFamily {
      id
      name
      ownerId
      createdAt
      members {
        userId
        role
        joinedAt
      }
    }
  }
`;

const HAS_FAMILY = gql`
  query HasFamily {
    hasFamily
  }
`;

const CREATE_FAMILY = gql`
  mutation CreateFamily($input: CreateFamilyInput!) {
    createFamily(input: $input) {
      success
      family {
        id
        name
        ownerId
        createdAt
        members {
          userId
          role
          joinedAt
        }
      }
      errorMessage
      validationErrors
    }
  }
`;

@Injectable({
  providedIn: 'root'
})
export class FamilyService {
  
  constructor(private apollo: Apollo) { }

  /**
   * Check if the current user is using the application for the first time
   */
  isFirstTimeUser(): Observable<boolean> {
    return this.apollo.query<{ isFirstTimeUser: boolean }>({
      query: IS_FIRST_TIME_USER,
      fetchPolicy: 'network-only' // Always fetch fresh data
    }).pipe(
      map(result => result.data.isFirstTimeUser),
      catchError(error => {
        console.error('Error checking first time user status:', error);
        return of(true); // Default to first time if error occurs
      })
    );
  }

  /**
   * Get comprehensive information about the user's first-time status and family membership
   */
  getFirstTimeUserInfo(): Observable<FirstTimeUserInfo> {
    return this.apollo.query<{ getFirstTimeUserInfo: FirstTimeUserInfo }>({
      query: GET_FIRST_TIME_USER_INFO,
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => result.data.getFirstTimeUserInfo),
      catchError(error => {
        console.error('Error getting first time user info:', error);
        return of({
          isFirstTime: true,
          hasFamily: false
        } as FirstTimeUserInfo);
      })
    );
  }

  /**
   * Check if the current user has a family
   */
  hasFamily(): Observable<boolean> {
    return this.apollo.query<{ hasFamily: boolean }>({
      query: HAS_FAMILY,
      fetchPolicy: 'cache-first'
    }).pipe(
      map(result => result.data.hasFamily),
      catchError(error => {
        console.error('Error checking family status:', error);
        return of(false);
      })
    );
  }

  /**
   * Get the current user's family information
   */
  getMyFamily(): Observable<FamilyInfo | null> {
    return this.apollo.query<{ getMyFamily: FamilyInfo | null }>({
      query: GET_MY_FAMILY,
      fetchPolicy: 'cache-first'
    }).pipe(
      map(result => result.data.getMyFamily),
      catchError(error => {
        console.error('Error getting family info:', error);
        return of(null);
      })
    );
  }

  /**
   * Create a new family
   */
  createFamily(familyName: string): Observable<CreateFamilyResult> {
    return this.apollo.mutate<{ createFamily: CreateFamilyResult }>({
      mutation: CREATE_FAMILY,
      variables: {
        input: {
          name: familyName.trim()
        }
      },
      // Update the cache after successful creation
      refetchQueries: [
        { query: GET_MY_FAMILY },
        { query: HAS_FAMILY },
        { query: GET_FIRST_TIME_USER_INFO },
        { query: IS_FIRST_TIME_USER }
      ]
    }).pipe(
      map(result => {
        if (result.data?.createFamily) {
          return result.data.createFamily;
        }
        return {
          success: false,
          errorMessage: 'Unbekannter Fehler beim Erstellen der Familie'
        };
      }),
      catchError(error => {
        console.error('Error creating family:', error);
        
        // Extract meaningful error message
        let errorMessage = 'Fehler beim Erstellen der Familie';
        if (error.graphQLErrors?.length > 0) {
          errorMessage = error.graphQLErrors[0].message;
        } else if (error.networkError) {
          errorMessage = 'Netzwerkfehler. Bitte versuchen Sie es später erneut.';
        }

        return of({
          success: false,
          errorMessage
        });
      })
    );
  }

  /**
   * Clear all family-related cache entries
   */
  clearFamilyCache(): void {
    this.apollo.client.cache.evict({ fieldName: 'getMyFamily' });
    this.apollo.client.cache.evict({ fieldName: 'hasFamily' });
    this.apollo.client.cache.evict({ fieldName: 'getFirstTimeUserInfo' });
    this.apollo.client.cache.evict({ fieldName: 'isFirstTimeUser' });
    this.apollo.client.cache.gc();
  }
}