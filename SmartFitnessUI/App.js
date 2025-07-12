// App.js
import React, { useContext } from 'react';
import { TouchableOpacity } from 'react-native';
import { NavigationContainer, getFocusedRouteNameFromRoute } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { navigationRef } from './navigation/NavigationService';
import { Ionicons } from '@expo/vector-icons';

import { AuthProvider, AuthContext } from './context/AuthContext';
import SignInScreen from './screens/SignInScreen';
import SignUpScreen from './screens/SignUpScreen';
import ForgotPasswordScreen from './screens/ForgotPasswordScreen';
import HomeScreen from './screens/HomeScreen';   // Activities screen
import MatchesScreen from './screens/MatchesScreen';
import LikesScreen from './screens/LikesScreen';
import MessagesScreen from './screens/MessagesScreen';
import SettingsScreen from './screens/SettingsScreen';
import ViewProfileScreen from './screens/ViewProfileScreen';
import ChatScreen from './screens/ChatScreen'; // Import ChatScreen
const AppStack = createNativeStackNavigator();
function AppNavigator() {
  return (
    <AppStack.Navigator>
      <AppStack.Screen
        name="Main"
        component={MainTabs}
        options={({ route, navigation }) => ({
          title: getHeaderTitle(route),
          headerTitleAlign: 'center',
          headerRight: () => (
            <TouchableOpacity onPress={() => navigation.navigate('Settings')} style={{ marginRight: 16 }}>
              <Ionicons name="settings-outline" size={24} color="#000" />
            </TouchableOpacity>
          ),
          headerTintColor: '#4CAF50',
        })}
      />
      <AppStack.Screen
        name="ViewProfile"
        component={ViewProfileScreen}
        options={{
          title: 'User Profile',
          headerTitleAlign: 'center',
          headerTintColor: '#4CAF50',
        }}
      />
      <AppStack.Screen
        name="Settings"
        component={SettingsScreen}
        options={{
          title: 'Profile Settings',
          headerTitleAlign: 'center',
          headerTintColor: '#4CAF50',
        }}
      />
      <AppStack.Screen
        name="ChatScreen"
        component={ChatScreen}
        options={{
          title: 'Chat',
          headerTitleAlign: 'center',
          headerTintColor: '#4CAF50',
        }}
      />
    </AppStack.Navigator>
  );
}
const AuthStack = createNativeStackNavigator();
function AuthNavigator() {
  return (
    <AuthStack.Navigator screenOptions={{ headerShown: false }}>
      <AuthStack.Screen name="SignIn" component={SignInScreen} />
      <AuthStack.Screen name="SignUp" component={SignUpScreen} />
      <AuthStack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
    </AuthStack.Navigator>
  );
}

export default function App() {
  const { userToken } = useContext(AuthContext);

  return (
    <NavigationContainer>
      {userToken ? <AppNavigator /> : <AuthNavigator />}
    </NavigationContainer>
  );
}
export function Root() {
  return (
    <AuthProvider>
      <App />
    </AuthProvider>
  );
}