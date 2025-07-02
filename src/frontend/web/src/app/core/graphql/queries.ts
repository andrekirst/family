import { gql } from 'apollo-angular';

// Authentication Mutations
export const DIRECT_LOGIN = gql`
  mutation DirectLogin($input: LoginInput!) {
    directLogin(input: $input) {
      accessToken
      refreshToken
      user {
        id
        email
        firstName
        lastName
        fullName
        preferredLanguage
        isActive
        createdAt
        lastLoginAt
      }
      errors
    }
  }
`;

export const INITIATE_LOGIN = gql`
  mutation InitiateLogin {
    initiateLogin {
      loginUrl
      state
    }
  }
`;

export const COMPLETE_LOGIN = gql`
  mutation CompleteLogin($input: LoginCallbackInput!) {
    completeLogin(input: $input) {
      accessToken
      refreshToken
      user {
        id
        email
        firstName
        lastName
        fullName
        preferredLanguage
        isActive
        createdAt
        lastLoginAt
      }
      errors
    }
  }
`;

export const REFRESH_TOKEN = gql`
  mutation RefreshToken($input: RefreshTokenInput!) {
    refreshToken(input: $input) {
      accessToken
      refreshToken
      errors
    }
  }
`;

export const LOGOUT = gql`
  mutation Logout($accessToken: String!) {
    logout(accessToken: $accessToken) {
      success
      errors
    }
  }
`;

// User Queries
export const GET_CURRENT_USER = gql`
  query GetCurrentUser {
    currentUser {
      id
      email
      firstName
      lastName
      fullName
      preferredLanguage
      isActive
      createdAt
      lastLoginAt
    }
  }
`;

export const GET_USER_BY_ID = gql`
  query GetUserById($id: String!) {
    userById(id: $id) {
      id
      email
      firstName
      lastName
      fullName
      preferredLanguage
      isActive
      createdAt
      lastLoginAt
    }
  }
`;

export const GET_ALL_USERS = gql`
  query GetAllUsers {
    users {
      id
      email
      firstName
      lastName
      fullName
      isActive
      createdAt
      lastLoginAt
    }
  }
`;